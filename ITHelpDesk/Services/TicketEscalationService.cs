using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ITHelpDesk.Services
{
    // ✅ Implement IHostedService and IDisposable
    public class TicketEscalationService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TicketEscalationService> _logger;
        private Timer _timer;

        public TicketEscalationService(IServiceScopeFactory scopeFactory, ILogger<TicketEscalationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TicketEscalationService started.");
            // Testing: run every 30 seconds instead of hourly
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("DoWork triggered at {Time}", DateTime.Now);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                var oneHourAgo = DateTime.Now.AddHours(-1);

                var escalatedTickets = await context.Tickets
                    .Where(t => t.AssignedTechnicianId == null
                             && t.CreatedAt <= oneHourAgo
                             && t.Priority.PriorityName.ToLower() == "high"
                             && !t.IsAutoEscalated)
                    .Include(t => t.Subcategory)
                        .ThenInclude(sc => sc.TechnicianGroups)
                    .Include(t => t.Priority)
                    .Include(t => t.Port)
                    .ToListAsync();


                _logger.LogInformation("Escalated tickets count: {Count}", escalatedTickets.Count);

                if (!escalatedTickets.Any())
                {
                    _logger.LogInformation("No tickets found for escalation.");
                    return;
                }

                var technicianGroupIds = escalatedTickets
                    .SelectMany(t => t.Subcategory.TechnicianGroups.Select(g => g.Id))
                    .Distinct()
                    .ToList();

                var portIds = escalatedTickets.Select(t => t.PortId).Distinct().ToList();

                var seniorTechnicians = await context.SeniorTechnicians
                    .Where(st => st.TechnicianGroupId.HasValue
                              && technicianGroupIds.Contains(st.TechnicianGroupId.Value)
                              && st.PortId.HasValue
                              && portIds.Contains(st.PortId.Value))
                    .ToListAsync();

                _logger.LogInformation("Matching senior technicians found: {Count}", seniorTechnicians.Count);

                var seniorMap = seniorTechnicians
                    .GroupBy(st => new { st.TechnicianGroupId, st.PortId })
                    .ToDictionary(g => g.Key, g => g.First());

                var seniorTicketPairs = new List<(SeniorTechnician, Ticket)>();

                foreach (var ticket in escalatedTickets)
                {
                    foreach (var groupId in ticket.Subcategory.TechnicianGroups.Select(g => g.Id))
                    {
                        var key = new { TechnicianGroupId = (int?)groupId, PortId = (int?)ticket.PortId };
                        if (seniorMap.TryGetValue(key, out var senior))
                        {
                            seniorTicketPairs.Add((senior, ticket));
                            break;
                        }
                    }
                }

                _logger.LogInformation("Tickets grouped for {Count} senior technicians", seniorTicketPairs.Select(x => x.Item1).Distinct().Count());

                var groupedBySenior = seniorTicketPairs.GroupBy(pair => pair.Item1);

                foreach (var group in groupedBySenior)
                {
                    var senior = group.Key;
                    var message = new StringBuilder();
                    message.AppendLine($"Dear {senior.FullName},");
                    message.AppendLine("<br/><br/>The following high-priority ticket(s) have not been attended for over an hour:");

                    foreach (var pair in group)
                    {
                        var ticket = pair.Item2;
                        message.AppendLine($"<br/>- Ticket {ticket.TicketNumber}, Created: {ticket.CreatedAt:g}");
                        ticket.IsAutoEscalated = true;
                    }

                    message.AppendLine("<br/><br/>Please take necessary action.");
                    message.AppendLine("<br/><br/>Regards,<br/>IT Helpdesk System");

                    _logger.LogInformation($"Sending escalation email to: {senior.Email}");

                    await emailSender.SendEmailAsync(
                        senior.Email,
                        "Escalation: Unassigned High-Priority Tickets",
                        message.ToString()
                    );
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Escalation process completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ticket escalation.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TicketEscalationService stopped.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}




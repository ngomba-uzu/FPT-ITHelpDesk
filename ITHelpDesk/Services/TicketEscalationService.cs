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
        private Timer _timer;

        public TicketEscalationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // Start the timer when the app starts
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Check every 1 minute
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                var oneHourAgo = DateTime.Now.AddHours(-1);

                var escalatedTickets = await context.Tickets
                    .Where(t => t.AssignedTechnicianId == null
                                && t.CreatedAt <= oneHourAgo
                                && t.Priority.PriorityName.ToLower() == "high"
                                && !t.IsAutoEscalated)
                    .Include(t => t.Subcategory)
                    .Include(t => t.Priority)
                    .Include(t => t.Port)
                    .ToListAsync();

                if (!escalatedTickets.Any())
                    return;

                var technicianGroupIds = escalatedTickets
                    .Select(t => t.Subcategory.TechnicianGroupId)
                    .Distinct()
                    .ToList();

                var portIds = escalatedTickets
                    .Select(t => t.PortId)
                    .Distinct()
                    .ToList();

                var seniorTechnicians = await context.SeniorTechnicians
                    .Where(st => st.TechnicianGroupId.HasValue
                               && technicianGroupIds.Contains(st.TechnicianGroupId.Value)
                               && st.PortId.HasValue
                               && portIds.Contains(st.PortId.Value))
                    .ToListAsync();

                var seniorMap = seniorTechnicians
                    .GroupBy(st => new { st.TechnicianGroupId, st.PortId })
                    .ToDictionary(
                        g => g.Key,
                        g => g.First()
                    );

                var seniorTicketPairs = new List<(SeniorTechnician, Ticket)>();
                foreach (var ticket in escalatedTickets)
                {
                    var key = new { TechnicianGroupId = (int?)ticket.Subcategory.TechnicianGroupId, PortId = (int?)ticket.PortId };
                    if (seniorMap.TryGetValue(key, out var senior))
                    {
                        seniorTicketPairs.Add((senior, ticket));
                    }
                }

                var groupedBySenior = seniorTicketPairs
                    .GroupBy(pair => pair.Item1);

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
                    message.AppendLine("<br/><br/>Regards,<br/>Ticketing System");

                    await emailSender.SendEmailAsync(senior.Email, "Escalation: Unassigned High-Priority Tickets", message.ToString());
                }

                await context.SaveChangesAsync();
            }
        }

        // Stop the timer when the app stops
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        // Dispose timer
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

using System.Diagnostics;
using System.Linq;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            ViewBag.Role = role;

            if (role == "Technical Support")
            {
                // Get technicians with formatted data
                var technicians = await _context.Technicians
                    .Include(t => t.TechnicianPorts)
                        .ThenInclude(tp => tp.Port)
                    .Include(t => t.TechnicianGroup)
                    .Include(t => t.Tickets)
                        .ThenInclude(t => t.Status)
                    .Select(t => new
                    {
                        t.Id,
                        t.FullName,
                        PortNames = string.Join(", ", t.TechnicianPorts.Select(tp => tp.Port.PortName)),
                        GroupName = t.TechnicianGroup != null ? t.TechnicianGroup.GroupName : "N/A",
                        WorkloadPercentage = Math.Min(
                            (int)((double)t.Tickets.Count(tkt => tkt.Status != null && tkt.Status.StatusName != "Closed") / 10 * 100),
                            100
                        )
                    })
                    .ToListAsync();
                // For Technical Support Dashboard
                ViewBag.EscalatedTickets = await _context.Tickets
                    .Include(t => t.Port) // Include Port
                    .Include(t => t.SeniorTechnician)
                    .Where(t => t.SeniorTechnicianId != null)
                    .OrderByDescending(t => t.EscalatedDate)
                    .Select(t => new
                    {
                        t.TicketNumber,
                        PortName = t.Port != null ? t.Port.PortName : "N/A", // Direct access
                        t.SeniorTechnicianResponse,
                        EscalatedDate = t.EscalatedDate.HasValue
                            ? t.EscalatedDate.Value.ToString("g")
                            : "N/A",
                        SeniorTech = t.SeniorTechnician != null
                            ? t.SeniorTechnician.FullName
                            : "N/A"
                    })
                    .ToListAsync();

                // Get ports with statistics
                ViewBag.Ports = await _context.Ports
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Status)
                    .Include(p => p.TechnicianPorts)
                    .Select(p => new
                    {
                        p.PortName,
                        ActiveTickets = p.Tickets.Count(t => t.Status != null && t.Status.StatusName != "Closed"),
                        TechnicianCount = p.TechnicianPorts.Count,
                        LastActivity = p.Tickets.Any()
                            ? p.Tickets.Max(t => t.CreatedAt).ToString("g")
                            : "N/A"
                    })
                    .ToListAsync();

                // Get categories with subcategory count
                ViewBag.Categories = await _context.Categories
                    .Select(c => new
                    {
                        c.CategoryName,
                        SubcategoryCount = c.Subcategories.Count
                    })
                    .ToListAsync();

                // Get subcategories with category names
                ViewBag.Subcategories = await _context.Subcategories
                    .Include(s => s.Category)
                    .Select(s => new
                    {
                        s.SubcategoryName,
                        CategoryName = s.Category != null ? s.Category.CategoryName : "N/A"
                    })
                    .ToListAsync();

                ViewBag.Technicians = technicians;
               /* ViewBag.EscalatedTickets = escalatedTickets;
                ViewBag.EscalatedTicketsCount = escalatedTickets.Count();*/
                ViewBag.TechniciansList = new SelectList(
                    await _context.Technicians.ToListAsync(),
                    "Id",
                    "FullName"
                );

                return View();


            }

            if (role == "Management")
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var localToday = localNow.Date;

                var allPorts = await _context.Ports
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Status)
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Priority)
                    .Include(p => p.TechnicianPorts)
                        .ThenInclude(tp => tp.Technician)
                    .ToListAsync();

                // Technician stats with null checks
                var technicianStats = await _context.Technicians
                    .Select(t => new
                    {
                        t.Id,
                        TicketsHandled = _context.Tickets.Count(ticket =>
                            ticket.AssignedTechnicianId == t.Id &&
                            ticket.Status.StatusName != "Closed"),
                        EscalatedTickets = _context.Tickets.Count(ticket =>
                            ticket.AssignedTechnicianId == t.Id &&
                            ticket.SeniorTechnicianId != null)
                    })
                    .ToDictionaryAsync(t => t.Id);

                // Get tickets from last 48 hours
                var recentTickets = allPorts
                    .SelectMany(p => p.Tickets)
                    .Where(t => t.CreatedAt >= DateTime.UtcNow.AddHours(-48))
                    .ToList();

                // Escalation tracking (using SeniorTechnicianId and open status)
                var activeEscalations = recentTickets
                    .Where(t => t.SeniorTechnicianId != null &&
                               t.Status.StatusName != "Closed")
                    .ToList();

                // High priority unattended tickets (unassigned + >1 hour old)
                var highPriorityUnattended = recentTickets
                    .Where(t => t.Priority != null &&
                               t.Priority.PriorityName.Equals("High", StringComparison.OrdinalIgnoreCase) &&
                               t.Status.StatusName == "Unassigned" &&
                               t.CreatedAt <= DateTime.UtcNow.AddHours(-1))
                    .ToList();

                ViewBag.ManagementStats = new
                {
                    TotalPorts = allPorts.Count,
                    ActivePorts = allPorts.Count(p => p.Tickets.Any()),
                    TotalTickets = allPorts.Sum(p => p.Tickets.Count),
                    ActiveTickets = allPorts.Sum(p => p.Tickets.Count(t => t.Status.StatusName != "Closed")),
                    ResolvedToday = allPorts.Sum(p => p.Tickets.Count(t =>
                        t.Status.StatusName == "Closed" &&
                        t.ClosedDate.HasValue &&
                        TimeZoneInfo.ConvertTimeFromUtc(t.ClosedDate.Value, timeZone).Date == localToday)),

                    ActiveTechnicians = allPorts
                        .SelectMany(p => p.TechnicianPorts)
                        .Select(tp => tp.TechnicianId)
                        .Distinct()
                        .Count(),

                    // Escalation metrics
                    EscalatedTickets = activeEscalations.Count,
                    HighPriorityEscalations = activeEscalations
                        .Count(t => t.Priority != null &&
                                  t.Priority.PriorityName.Equals("High", StringComparison.OrdinalIgnoreCase)),
                    UnattendedHighPriority = highPriorityUnattended.Count
                };

                ViewBag.Ports = allPorts.Select(p => new
                {
                    p.Id,
                    p.PortName,
                    OpenTickets = p.Tickets.Count(t => t.Status.StatusName == "Unassigned"),
                    PendingTickets = p.Tickets.Count(t => t.Status.StatusName == "Assigned"),
                    ClosedTickets = p.Tickets.Count(t => t.Status.StatusName == "Closed"),
                    EscalatedTickets = p.Tickets.Count(t => t.SeniorTechnicianId != null),
                    TechnicianUtilization = p.TechnicianPorts.Any()
                        ? Math.Round((double)p.Tickets.Count(t => t.Status.StatusName != "Closed") /
                                    p.TechnicianPorts.Count, 1)
                        : 0,
                    Technicians = p.TechnicianPorts
                        .OrderBy(tp => tp.Technician.FullName)
                        .Select(tp => new
                        {
                            tp.Technician.Id,
                            Name = tp.Technician.FullName,
                            Initials = GetInitials(tp.Technician.FullName),
                            TicketsHandled = technicianStats.TryGetValue(tp.Technician.Id, out var stats)
                                ? stats.TicketsHandled
                                : 0,
                            EscalatedCount = technicianStats.TryGetValue(tp.Technician.Id, out var eStats)
                                ? eStats.EscalatedTickets
                                : 0
                        })
                        .ToList()
                }).ToList();

                return View();
            }

            if (role == "Technician")
            {
                var technician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (technician != null)
                {
                    int technicianId = technician.Id;
                    int technicianGroupId = technician.TechnicianGroupId;

                    var assignedTickets = _context.Tickets
                        .Include(t => t.Status)
                        .Where(t => t.AssignedTechnicianId == technicianId);

                    var unassignedGroupTickets = _context.Tickets
                        .Include(t => t.Status)
                        .Include(t => t.Subcategory)
                        .Where(t =>
                            t.AssignedTechnicianId == null &&
                            t.Subcategory != null &&
                            t.Subcategory.TechnicianGroupId == technicianGroupId &&
                            t.Status != null &&
                            t.Status.StatusName == "Unassigned");

                    ViewBag.TechnicianStats = new
                    {
                        Opened = await unassignedGroupTickets.CountAsync(),
                        Pending = await assignedTickets.CountAsync(t => t.Status.StatusName == "Assigned"),
                        Closed = await assignedTickets.CountAsync(t => t.Status.StatusName == "Closed"),
                        Escalated = await assignedTickets.CountAsync(t => t.SeniorTechnicianId != null),
                        Total = await assignedTickets.CountAsync() + await unassignedGroupTickets.CountAsync()
                    };

                    var assignedPorts = await _context.TechnicianPorts
                        .Where(tp => tp.TechnicianId == technicianId)
                        .Include(tp => tp.Port)
                        .ToListAsync();

                    ViewBag.PortStats = assignedPorts.Any() ? new
                    {
                        PortName = assignedPorts.First().Port.PortName,
                        Opened = await _context.Tickets.CountAsync(t =>
                            t.PortId == assignedPorts.First().PortId &&
                            t.Status.StatusName == "Unassigned"),
                        Pending = await _context.Tickets.CountAsync(t =>
                            t.PortId == assignedPorts.First().PortId &&
                            t.Status.StatusName == "Assigned"),
                        Closed = await _context.Tickets.CountAsync(t =>
                            t.PortId == assignedPorts.First().PortId &&
                            t.Status.StatusName == "Closed"),
                        Escalated = await _context.Tickets.CountAsync(t =>
                            t.PortId == assignedPorts.First().PortId &&
                            t.SeniorTechnicianId != null),
                        Total = await _context.Tickets.CountAsync(t =>
                            t.PortId == assignedPorts.First().PortId)
                    } : new
                    {
                        PortName = "No Port Assigned",
                        Opened = 0,
                        Pending = 0,
                        Closed = 0,
                        Escalated = 0,
                        Total = 0
                    };

                    ViewBag.TechnicianTickets = await assignedTickets
                        .Include(t => t.Status)
                        .Include(t => t.Priority)
                        .Include(t => t.Port)
                        .Include(t => t.Category)
                        .Include(t => t.Subcategory)
                        .OrderByDescending(t => t.CreatedAt)
                        .Take(10)
                        .Select(t => new
                        {
                            t.TicketNumber,
                            t.RequesterName,
                            t.Email,
                            Port = new { PortName = t.Port != null ? t.Port.PortName : "N/A" },
                            Category = t.Category != null ? t.Category.CategoryName : "N/A",
                            Subcategory = t.Subcategory != null ? t.Subcategory.SubcategoryName : "N/A",
                            Priority = t.Priority != null ? t.Priority.PriorityName : "N/A",
                            Status = t.Status != null ? t.Status.StatusName : "N/A"
                        })
                        .ToListAsync();
                }
                else
                {
                    ViewBag.TechnicianStats = new { Opened = 0, Pending = 0, Closed = 0, Escalated = 0, Total = 0 };
                    ViewBag.PortStats = new
                    {
                        PortName = "No Port Assigned",
                        Opened = 0,
                        Pending = 0,
                        Closed = 0,
                        Escalated = 0,
                        Total = 0
                    };
                    ViewBag.TechnicianTickets = new List<object>();
                }
            }
            else
            {
                var userTickets = _context.Tickets.Where(t => t.CreatedBy == user.Id);
                ViewBag.TotalTickets = await userTickets.CountAsync();
                ViewBag.PendingTickets = await userTickets.CountAsync(t => t.Status.StatusName == "Assigned");
                ViewBag.ReceivedTickets = await userTickets.CountAsync(t => t.Status.StatusName == "Closed");
                ViewBag.RecentTickets = await userTickets
                    .Include(t => t.Status)
                    .Include(t => t.Port)
                    .Include(t => t.Category)
                    .Include(t => t.Subcategory)
                    .Include(t => t.Priority)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }

            return View();
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";
            var names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return names.Length switch
            {
                0 => "??",
                1 => names[0][0].ToString().ToUpper(),
                _ => $"{names[0][0]}{names[^1][0]}".ToUpper()
            };
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
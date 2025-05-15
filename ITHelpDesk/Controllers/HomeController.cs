using System.Diagnostics;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            ViewBag.Role = role;

            // Common statistics for all roles
            ViewBag.TotalTickets = await _context.Tickets.CountAsync();
            ViewBag.PendingTickets = await _context.Tickets
                .CountAsync(t => t.Status != null && t.Status.StatusName == "Assigned");
            ViewBag.ReceivedTickets = await _context.Tickets
                .CountAsync(t => t.Status != null && t.Status.StatusName == "Closed");

            // Loading Recent Tickets with related data
            ViewBag.RecentTickets = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Port)
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .Include(t => t.Priority)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Technician-specific statistics
            if (role == "Technician")
            {
                // Get the technician record
                var technician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (technician != null)
                {
                    int technicianId = technician.Id;
                    int technicianGroupId = technician.TechnicianGroupId;

                    // Tickets assigned to this technician
                    var assignedTickets = _context.Tickets
                        .Include(t => t.Status)
                        .Where(t => t.AssignedTechnicianId == technicianId);

                    // Tickets unassigned but routed to technician's group via Subcategory
                    var unassignedGroupTickets = _context.Tickets
                        .Include(t => t.Status)
                        .Include(t => t.Subcategory)
                        .Where(t =>
                            t.AssignedTechnicianId == null &&
                            t.Subcategory != null &&
                            t.Subcategory.TechnicianGroupId == technicianGroupId &&
                            t.Status != null &&
                            t.Status.StatusName == "Unassigned");

                    int openedCount = await unassignedGroupTickets.CountAsync();
                    int pendingCount = await assignedTickets.CountAsync(t => t.Status != null && t.Status.StatusName == "Assigned");
                    int closedCount = await assignedTickets.CountAsync(t => t.Status != null && t.Status.StatusName == "Closed");
                    int escalatedCount = await assignedTickets.CountAsync(t => t.SeniorTechnicianId != null);
                    int totalCount = await assignedTickets.CountAsync() + openedCount; // ? Fixed line

                    ViewBag.TechnicianStats = new
                    {
                        Opened = openedCount,
                        Pending = pendingCount,
                        Closed = closedCount,
                        Escalated = escalatedCount,
                        Total = totalCount
                    };
                



                // Get assigned ports for this technician
                var assignedPorts = await _context.TechnicianPorts
                        .Where(tp => tp.TechnicianId == technicianId)
                        .Include(tp => tp.Port)
                        .ToListAsync();

                    if (assignedPorts.Any())
                    {
                        var primaryPort = assignedPorts.First().Port;
                        ViewBag.AssignedPort = primaryPort;

                        var portTickets = _context.Tickets
                            .Where(t => t.PortId == primaryPort.Id &&
                                        t.CreatedAt >= DateTime.Now.AddMonths(-1));

                        ViewBag.PortStats = new
                        {
                            PortName = primaryPort.PortName,
                            Opened = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Unassigned"),
                            Pending = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Assigned"),
                            Closed = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Closed"),
                            Escalated = await portTickets
                                .CountAsync(t => t.SeniorTechnicianId != null),
                            Total = await portTickets.CountAsync()
                        };
                    }
                    else
                    {
                        ViewBag.PortStats = new
                        {
                            PortName = "No Port Assigned",
                            Opened = 0,
                            Pending = 0,
                            Closed = 0,
                            Escalated = 0,
                            Total = 0
                        };
                    }

                    // Technician's assigned tickets with related data
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
                            TicketNumber = t.TicketNumber,
                            RequesterName = t.RequesterName,
                            RequesterEmail = t.Email,
                            Site = t.Port != null ? t.Port.PortName : "N/A",
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

            return View();
        }


        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
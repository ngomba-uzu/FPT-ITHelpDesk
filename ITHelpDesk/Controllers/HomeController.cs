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

                    // Get technician's assigned tickets
                    var technicianTickets = _context.Tickets
                        .Where(t => t.AssignedTechnicianId == technicianId);

                    // Technician activities
                    ViewBag.TechnicianStats = new
                    {
                        Opened = await technicianTickets
                            .CountAsync(t => t.Status != null && t.Status.StatusName == "Open"),
                        Pending = await technicianTickets
                            .CountAsync(t => t.Status != null && t.Status.StatusName == "Pending"),
                        Closed = await technicianTickets
                            .CountAsync(t => t.Status != null && t.Status.StatusName == "Closed"),
                        Escalated = await technicianTickets
                            .CountAsync(t => t.SeniorTechnicianId != null)
                    };

                    // Get all ports assigned to this technician
                    var assignedPorts = await _context.TechnicianPorts
                        .Where(tp => tp.TechnicianId == technicianId)
                        .Include(tp => tp.Port)
                        .ToListAsync();

                    ViewBag.AssignedPorts = assignedPorts.Select(p => p.Port).ToList();

                    if (assignedPorts.Any())
                    {
                        // Get all port IDs for this technician
                        var portIds = assignedPorts.Select(p => p.PortId).ToList();

                        // Port monthly activities (aggregate across all assigned ports)
                        var portTickets = _context.Tickets
                            .Where(t => portIds.Contains(t.PortId) &&
                                      t.CreatedAt >= DateTime.Now.AddMonths(-1));

                        ViewBag.PortStats = new
                        {
                            Opened = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Open"),
                            Pending = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Pending"),
                            Closed = await portTickets
                                .CountAsync(t => t.Status != null && t.Status.StatusName == "Closed"),
                            Escalated = await portTickets
                                .CountAsync(t => t.SeniorTechnicianId != null),
                            TotalPorts = assignedPorts.Count
                        };

                        // For technicians with multiple ports, we'll also include per-port breakdown
                        if (assignedPorts.Count > 1)
                        {
                            ViewBag.PortBreakdown = await _context.Tickets
                                .Where(t => portIds.Contains(t.PortId) &&
                                          t.CreatedAt >= DateTime.Now.AddMonths(-1))
                                .GroupBy(t => t.PortId)
                                .Select(g => new
                                {
                                    PortId = g.Key,
                                    PortName = g.First().Port.PortName,
                                    Opened = g.Count(t => t.Status != null && t.Status.StatusName == "Open"),
                                    Pending = g.Count(t => t.Status != null && t.Status.StatusName == "Pending"),
                                    Closed = g.Count(t => t.Status != null && t.Status.StatusName == "Closed"),
                                    Escalated = g.Count(t => t.SeniorTechnicianId != null)
                                })
                                .ToListAsync();
                        }
                    }
                    else
                    {
                        ViewBag.PortStats = new { Opened = 0, Pending = 0, Closed = 0, Escalated = 0, TotalPorts = 0 };
                    }

                    // Technician's assigned tickets with related data
                    ViewBag.TechnicianTickets = await technicianTickets
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
                            Priority = t.Priority != null ? t.Priority.PriorityName : "N/A"
                        })
                        .ToListAsync();
                }
                else
                {
                    // Default values if technician record not found
                    ViewBag.TechnicianStats = new { Opened = 0, Pending = 0, Closed = 0, Escalated = 0 };
                    ViewBag.PortStats = new { Opened = 0, Pending = 0, Closed = 0, Escalated = 0, TotalPorts = 0 };
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
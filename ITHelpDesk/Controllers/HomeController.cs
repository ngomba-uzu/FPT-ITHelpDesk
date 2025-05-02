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
            ViewBag.Role = roles.FirstOrDefault();

            // Total Tickets, Pending Tickets, and Closed Tickets
            ViewBag.TotalTickets = _context.Tickets.Count();
            ViewBag.PendingTickets = _context.Tickets.Count(t => t.Status != null && t.Status.StatusName == "Assigned");
            ViewBag.ReceivedTickets = _context.Tickets.Count(t => t.Status != null && t.Status.StatusName == "Closed");

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

            return View();
        }



        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
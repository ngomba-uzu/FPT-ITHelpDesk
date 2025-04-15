using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TechniciansController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Technicians
        public async Task<IActionResult> Index()
        {
            var technicians = await _context.Technicians
                                    .Include(t => t.TechnicianGroup) // Include the group
                                    .ToListAsync();

            return View(technicians);
        }




        // GET: Technicians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technician == null)
            {
                return NotFound();
            }

            return View(technician);
        }

        // GET: Technicians/Create
        public IActionResult Create()
        {
            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups.ToList(), "Id", "GroupName");
            return View();
        }

        // POST: Technicians/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Technician technician)
        {
            if (ModelState.IsValid)
            {
                // Assign the logged-in user's ID to the UserId field
                technician.UserId = _userManager.GetUserId(User);

                _context.Add(technician);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups.ToList(), "Id", "GroupName", technician.TechnicianGroupId);
            return View(technician);
        }

        // GET: Technicians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null)
            {
                return NotFound();
            }
            return View(technician);
        }

        // POST: Technicians/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email")] Technician technician)
        {
            if (id != technician.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(technician);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianExists(technician.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(technician);
        }

        // GET: Technicians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technician == null)
            {
                return NotFound();
            }

            return View(technician);
        }

        // POST: Technicians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);
            if (technician != null)
            {
                _context.Technicians.Remove(technician);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TechnicianExists(int id)
        {
            return _context.Technicians.Any(e => e.Id == id);
        }
        // Landing page with buttons
        public IActionResult MyTickets()
        {
            var technicianId = _context.Technicians
                .FirstOrDefault(t => t.User.Id == _userManager.GetUserId(User))?.Id;

            var myTickets = _context.Tickets
                .Include(t => t.Subcategory)
                .Where(t => t.AssignedTechnicianId == technicianId)
                .ToList();

            return View(myTickets);
        }


        // Shows tickets assigned to this technician
        public async Task<IActionResult> AssignedTickets()
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            var tickets = await _context.Tickets
                .Where(t => t.AssignedTechnicianId == technician.Id && t.Status.StatusName != "Closed")
                .Include(t => t.Status)
                .ToListAsync();

            return View(tickets);
        }

        // Shows unassigned tickets relevant to this technician’s group
        public async Task<IActionResult> UnassignedTickets()
        {
            // Get current technician by email
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            if (technician == null)
            {
                return NotFound("Technician not found.");
            }

            // Get subcategories linked to technician's group
            var subcategoryIds = await _context.Subcategories
                .Where(s => s.TechnicianGroupId == technician.TechnicianGroupId)
                .Select(s => s.Id)
                .ToListAsync();

            // Get unassigned tickets linked to those subcategories
            var tickets = await _context.Tickets
                .Where(t => t.AssignedTechnicianId == null && subcategoryIds.Contains(t.SubcategoryId))
                .Include(t => t.Subcategory)
                .Include(t => t.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Port)
                .ToListAsync();

            return View(tickets);
        }


        [HttpPost]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            ticket.AssignedTechnicianId = technician.Id;
            ticket.StatusChangedAt = DateTime.Now;

            // Optional: Update ticket status to "Assigned"
            var assignedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Assigned");
            if (assignedStatus != null)
            {
                ticket.StatusId = assignedStatus.Id;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("UnassignedTickets", "Technicians");
        }


        // Shows closed tickets handled by this technician
        public async Task<IActionResult> ClosedTickets()
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            var tickets = await _context.Tickets
                .Where(t => t.AssignedTechnicianId == technician.Id && t.Status.StatusName == "Closed")
                .Include(t => t.Status)
                .ToListAsync();

            return View(tickets);
        }


    }
}

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
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ITHelpDesk.Controllers
{
    public class SeniorTechniciansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SeniorTechniciansController> _logger;

        public SeniorTechniciansController(ApplicationDbContext context, IEmailSender emailSender, ILogger<SeniorTechniciansController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: SeniorTechnicians
        public async Task<IActionResult> Index()
        {
            var seniorTechnicians = await _context.SeniorTechnicians
                .Include(s => s.TechnicianGroup)
                .Include(s => s.Port)
                .ToListAsync();

            return View(seniorTechnicians);
        }

        // GET: SeniorTechnicians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var seniorTechnician = await _context.SeniorTechnicians
                .Include(s => s.TechnicianGroup)
                .Include(s => s.Port)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (seniorTechnician == null) return NotFound();

            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Create
        public IActionResult Create()
        {
            ViewBag.TechnicianGroupId = new SelectList(_context.TechnicianGroups, "Id", "GroupName");
            ViewBag.PortId = new SelectList(_context.Ports, "Id", "PortName");
            return View();
        }

        // POST: SeniorTechnicians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Email,TechnicianGroupId,PortId")] SeniorTechnician seniorTechnician)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seniorTechnician);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TechnicianGroupId = new SelectList(_context.TechnicianGroups, "Id", "GroupName", seniorTechnician.TechnicianGroupId);
            ViewBag.PortId = new SelectList(_context.Ports, "Id", "PortName", seniorTechnician.PortId);
            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var seniorTechnician = await _context.SeniorTechnicians.FindAsync(id);
            if (seniorTechnician == null) return NotFound();

            ViewBag.TechnicianGroupId = new SelectList(_context.TechnicianGroups, "Id", "GroupName", seniorTechnician.TechnicianGroupId);
            ViewBag.PortId = new SelectList(_context.Ports, "Id", "PortName", seniorTechnician.PortId);
            return View(seniorTechnician);
        }

        // POST: SeniorTechnicians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,TechnicianGroupId,PortId")] SeniorTechnician seniorTechnician)
        {
            if (id != seniorTechnician.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seniorTechnician);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeniorTechnicianExists(seniorTechnician.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TechnicianGroupId = new SelectList(_context.TechnicianGroups, "Id", "GroupName", seniorTechnician.TechnicianGroupId);
            ViewBag.PortId = new SelectList(_context.Ports, "Id", "PortName", seniorTechnician.PortId);
            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var seniorTechnician = await _context.SeniorTechnicians
                .Include(s => s.TechnicianGroup)
                .Include(s => s.Port)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (seniorTechnician == null) return NotFound();

            return View(seniorTechnician);
        }

        // POST: SeniorTechnicians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seniorTechnician = await _context.SeniorTechnicians.FindAsync(id);
            if (seniorTechnician != null)
            {
                _context.SeniorTechnicians.Remove(seniorTechnician);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SeniorTechnicianExists(int id)
        {
            return _context.SeniorTechnicians.Any(e => e.Id == id);
        }


        public async Task<IActionResult> ReviewEscalatedTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Department)
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .Include(t => t.Port)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }
        [HttpPost]
        public async Task<IActionResult> SubmitSeniorResponse(int TicketId, string Response)
        {
            var ticket = await _context.Tickets
                .Include(t => t.AssignedTechnician) // 👈 include technician for email access
                .FirstOrDefaultAsync(t => t.Id == TicketId);

            if (ticket == null)
                return NotFound();

            ticket.SeniorTechnicianResponse = Response;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            // ✅ Send email to the assigned technician
            if (ticket.AssignedTechnician != null && !string.IsNullOrWhiteSpace(ticket.AssignedTechnician.Email))
            {
                var emailBody = $@"
            <p>Dear {ticket.AssignedTechnician.FullName},</p>
            <p>The senior technician has responded to the escalated ticket.</p>
            <p>You can now proceed to the Resolution section to review the response and continue resolving the ticket.</p>
            <p><strong>Ticket Number:</strong> {ticket.TicketNumber}</p>
            <p>Regards,<br/>HelpDesk System</p>";

                try
                {
                    await _emailSender.SendEmailAsync(
                        ticket.AssignedTechnician.Email.Trim(),
                        "Senior Technician Response Submitted",
                        emailBody
                    );

                    _logger.LogInformation("Email sent to technician {Email}", ticket.AssignedTechnician.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to technician {Email}", ticket.AssignedTechnician.Email);
                    TempData["Error"] = "Response submitted, but email notification failed.";
                }
            }

            return Content("Response submitted successfully. The technician has been notified.");
        }
    }
}

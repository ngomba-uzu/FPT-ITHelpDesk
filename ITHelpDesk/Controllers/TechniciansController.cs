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
/*using ITHelpDesk.Models.ViewModels;*/
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using ITHelpDesk.Models.ViewModels;
using ITHelpDesk.Services;

namespace ITHelpDesk.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TechniciansController> _logger;

        public TechniciansController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IWebHostEnvironment env, ILogger<TechniciansController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _env = env;
            _logger = logger;
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

        public async Task<IActionResult> MyTickets()
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            if (technician == null)
            {
                return Unauthorized(); // or redirect with error message
            }

            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .Include(t => t.Port)
                .Include(t => t.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.AssignedTechnician)
                .Where(t => t.AssignedTechnicianId == technician.Id && t.Status.StatusName == "Assigned")
                .ToListAsync();

            ViewBag.Technicians = await _context.Technicians.ToListAsync();

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


        // POST: AssignToMe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Get the currently logged-in technician based on email
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            if (technician == null)
            {
                ModelState.AddModelError(string.Empty, "Technician profile not found.");
                return RedirectToAction("UnassignedTickets", "Technicians");
            }

            // Assign the ticket
            ticket.AssignedTechnicianId = technician.Id;
            ticket.StatusChangedAt = DateTime.Now;

            // Set ticket status to 'Assigned'
            var assignedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Assigned");
            if (assignedStatus != null)
            {
                ticket.StatusId = assignedStatus.Id;
            }

            await _context.SaveChangesAsync();

            // Redirect to the assigned tickets view for the technician
            return RedirectToAction("MyTickets", "Technicians");
        }

        // GET: Technicians/Resolution/5
        public async Task<IActionResult> Resolution(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Only block if status is still Closed
            if (ticket.Status.StatusName == "Closed")
            {
                return RedirectToAction("ClosedTickets");
            }

            var model = new ResolutionViewModel
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                SeniorTechnicianResponse = ticket.SeniorTechnicianResponse,
                Description = ticket.Description,
                StatusList = await _context.Status.ToListAsync()
            };

            return View(model);
        }


        // POST: Technicians/Resolution
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolution(ResolutionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ticket = await _context.Tickets.FindAsync(model.TicketId);
                if (ticket == null)
                    return NotFound();

                ticket.Resolution = model.Resolution;

                // Save the uploaded file if exists
                if (model.Document != null && model.Document.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "documents");
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, model.Document.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Document.CopyToAsync(stream);
                    }

                    ticket.FileName = "/documents/" + model.Document.FileName;
                }

                // Set ticket status to Closed
                var closedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Closed");
                if (closedStatus == null)
                {
                    closedStatus = new Status { StatusName = "Closed" };
                    _context.Status.Add(closedStatus);
                    await _context.SaveChangesAsync();
                }

                ticket.StatusId = closedStatus.Id;
                ticket.ClosedDate = DateTime.Now;
                ticket.IsResolutionAcknowledged = false;

                _context.Update(ticket);
                await _context.SaveChangesAsync();

                // Send acknowledgment email
                var subject = $"Ticket {ticket.TicketNumber} Closed - Was it resolved?";
                var body = $@"
            <p>Dear {ticket.RequesterName},</p>
            <p>Your ticket <strong>{ticket.TicketNumber}</strong> has been marked as resolved.</p>
            <p>Are you satisfied with the resolution?</p>
            <p>
                <a href='https://localhost:44388/Technicians/Acknowledge?id={ticket.Id}&acknowledge=Yes'>✅ Yes</a> &nbsp;
                <a href='https://localhost:44388/Technicians/Acknowledge?id={ticket.Id}&acknowledge=No'>❌ No</a>
            </p>
            <p>Thank you,<br/>IT HelpDesk Team</p>";

                await _emailSender.SendEmailAsync(ticket.Email, subject, body);

                return RedirectToAction("ClosedTickets");
            }

            // Log errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            model.StatusList = await _context.Status.ToListAsync();
            return View(model);
        }

        // Acknowledge via email
        [HttpGet]
        public async Task<IActionResult> Acknowledge(int id, string acknowledge)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            if (acknowledge == "Yes")
            {
                // Keep it closed and acknowledge
                ticket.IsResolutionAcknowledged = true;
            }
            else if (acknowledge == "No")
            {
                // Move back to assigned
                var assignedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Assigned");
                if (assignedStatus == null)
                {
                    assignedStatus = new Status { StatusName = "Assigned" };
                    _context.Status.Add(assignedStatus);
                    await _context.SaveChangesAsync();
                }

                ticket.StatusId = assignedStatus.Id;
                ticket.IsResolutionAcknowledged = false;
            }

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return Content("Thank you for your response. You may now close this window.");
        }


        // GET: Technicians/ClosedTickets
        public async Task<IActionResult> ClosedTickets()
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Email == User.Identity.Name);

            if (technician == null)
            {
                return Unauthorized(); // or redirect with error
            }

            var closedStatusIds = await _context.Status
                .Where(s => s.StatusName == "Closed" || s.StatusName == "Awaiting Acknowledgment")
                .Select(s => s.Id)
                .ToListAsync();

            var closedTickets = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.AssignedTechnician)
                .Where(t => closedStatusIds.Contains(t.StatusId) && t.ClosedByTechnicianId == technician.Id)
                .ToListAsync();

            return View(closedTickets);
        }


        [HttpPost]
        public async Task<IActionResult> ReopenTicket(int ticketId, string ReopenReason)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            var assignedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Assigned");
            if (assignedStatus == null)
            {
                return BadRequest("Assigned status not found.");
            }

            // Re-open the ticket
            ticket.StatusId = assignedStatus.Id;
            ticket.ReopenReason = ReopenReason;
            ticket.ClosedDate = null;
            ticket.IsResolutionAcknowledged = false; // 🟢 Reset this

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTickets");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignTicket(int TicketId, int TechnicianId, string ReassignReason)
        {
            var ticket = await _context.Tickets.FindAsync(TicketId);
            if (ticket == null) return NotFound();

            ticket.AssignedTechnicianId = TechnicianId;
            ticket.ReassignReason = ReassignReason;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTickets");
        }


        public async Task<IActionResult> Escalate(int id)
        {
            var model = new EscalateTicketViewModel
            {
                TicketId = id,
                SeniorTechnicians = await _context.SeniorTechnicians.ToListAsync()
            };

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Escalate(EscalateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.SeniorTechnicians = await _context.SeniorTechnicians.ToListAsync();
                return View(model);
            }

            if (model.SeniorTechnicianId == 0)
            {
                ModelState.AddModelError("SeniorTechnicianId", "Please select a senior technician.");
                model.SeniorTechnicians = await _context.SeniorTechnicians.ToListAsync();
                return View(model);
            }

            var ticket = await _context.Tickets.FindAsync(model.TicketId);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket with ID {TicketId} not found.", model.TicketId);
                return NotFound();
            }

            var senior = await _context.SeniorTechnicians.FindAsync(model.SeniorTechnicianId);
            if (senior == null)
            {
                _logger.LogWarning("Senior technician with ID {TechId} not found.", model.SeniorTechnicianId);
                return NotFound("Selected Senior Technician not found.");
            }

            if (string.IsNullOrWhiteSpace(senior.Email))
            {
                _logger.LogWarning("Senior technician {FullName} (ID: {Id}) has no email.", senior.FullName, senior.Id);
                ModelState.AddModelError("", "Selected senior technician does not have a valid email address.");
                model.SeniorTechnicians = await _context.SeniorTechnicians.ToListAsync();
                return View(model);
            }

            // Update ticket
            ticket.SeniorTechnicianId = model.SeniorTechnicianId;
            ticket.EscalateReason = model.EscalateReason;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            // Generate review link
            var url = Url.Action("ReviewEscalatedTicket", "SeniorTechnicians", new { id = ticket.Id }, Request.Scheme);

            // Compose email
            var subject = "New Escalated Ticket Assigned to You";
            var message = $@"
<p>Dear {senior.FullName},</p>
<p>A ticket has been escalated for your review.</p>
<p><strong>Reason:</strong> {model.EscalateReason}</p>
<p><a href='{url}'>Click here to view and respond to the ticket</a></p>
<p>Regards,<br/>IT HelpDesk System</p>";

            try
            {
                await _emailSender.SendEmailAsync(senior.Email.Trim(), subject, message);
                _logger.LogInformation("Escalation email successfully sent to {Email}", senior.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send escalation email to {Email}", senior.Email);
                TempData["Error"] = "The ticket was escalated, but the email failed to send.";
            }

            return RedirectToAction("MyTickets");
        }



    }
}


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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using ITHelpDesk.Models.ViewModels;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using MailKit.Search;
using System.Text;

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
        private readonly NotificationService _notificationService;

        public TechniciansController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IWebHostEnvironment env, ILogger<TechniciansController> logger, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _env = env;
            _logger = logger;
            _notificationService = notificationService;
        }

        // GET: Technicians
        public async Task<IActionResult> Index()
        {
            var technicians = await _context.Technicians
                .Include(t => t.TechnicianGroup)
                .Include(t => t.TechnicianPorts)
                    .ThenInclude(tp => tp.Port)
                .Include(t => t.User)
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
            // Initialize a list to hold the user data
            var users = new List<dynamic>();

            // Loop through the users in the context
            foreach (var user in _context.Users)
            {
                var appUser = user as ApplicationUser;  // Cast to ApplicationUser

                // If the cast is successful, add the user data to the list
                if (appUser != null)
                {
                    users.Add(new { appUser.Id, appUser.FullName, appUser.Email });
                }
            }

            // ViewBag for the dropdown list, ensuring it's of the correct type
            ViewBag.Users = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),  // `Value` should be a string
                Text = u.FullName         // `Text` should display FullName
            }).ToList();

            // ViewBag for TechnicianGroups and Ports
            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups, "Id", "GroupName");
            ViewBag.Ports = new SelectList(_context.Ports, "Id", "PortName");

            // ViewBag for serialized JSON users data for use in JavaScript
            ViewBag.UsersJson = JsonConvert.SerializeObject(users);

            // Return the view
            return View();
        }




        // POST: Technicians/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Technician technician, List<int> selectedPorts)
        {
            if (ModelState.IsValid)
            {
                technician.TechnicianPorts = selectedPorts.Select(portId => new TechnicianPort
                {
                    PortId = portId,
                    TechnicianId = technician.Id
                }).ToList();

                _context.Add(technician);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Safely cast to ApplicationUser to access FullName
            var users = new List<dynamic>();
            foreach (var user in _context.Users)
            {
                var appUser = user as ApplicationUser;
                if (appUser != null)
                {
                    users.Add(new { appUser.Id, appUser.FullName, appUser.Email });
                }
            }

            ViewBag.Users = users;
            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups, "Id", "GroupName", technician.TechnicianGroupId);
            ViewBag.Ports = new SelectList(_context.Ports, "Id", "PortName");
            return View(technician);
        }



        // GET: Technicians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.Technicians
                .Include(t => t.TechnicianPorts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            // Get selected port IDs from TechnicianPorts
            var selectedPortIds = technician.TechnicianPorts.Select(tp => tp.PortId).ToList();

            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups.ToList(), "Id", "GroupName", technician.TechnicianGroupId);
            ViewBag.Ports = new MultiSelectList(_context.Ports.ToList(), "Id", "PortName", selectedPortIds);


            return View(technician);
        }



        // POST: Technicians/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Technician technician, List<int> selectedPorts)
        {
            if (id != technician.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTechnician = await _context.Technicians
                        .Include(t => t.TechnicianPorts)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTechnician == null)
                        return NotFound();

                    // Update main fields
                    existingTechnician.FullName = technician.FullName;
                    existingTechnician.Email = technician.Email;
                    existingTechnician.TechnicianGroupId = technician.TechnicianGroupId;

                    // Remove old port links
                    _context.TechnicianPorts.RemoveRange(existingTechnician.TechnicianPorts);

                    // Add updated port links
                    if (selectedPorts != null && selectedPorts.Any())
                    {
                        foreach (var portId in selectedPorts)
                        {
                            _context.TechnicianPorts.Add(new TechnicianPort
                            {
                                TechnicianId = existingTechnician.Id,
                                PortId = portId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianExists(technician.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Rebind dropdowns if model state is invalid
            ViewBag.TechnicianGroups = new SelectList(_context.TechnicianGroups.ToList(), "Id", "GroupName", technician.TechnicianGroupId);
            ViewBag.Ports = new MultiSelectList(_context.Ports.ToList(), "Id", "PortName", selectedPorts);

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


        public async Task<IActionResult> MyTickets(string searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (technician == null)
            {
                return Unauthorized();
            }

            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.AssignedTechnician)
                .Where(t => t.AssignedTechnicianId == technician.Id && t.Status.StatusName == "Assigned");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.TicketNumber.Contains(searchTerm) ||
                    t.RequesterName.Contains(searchTerm) ||
                    t.Email.Contains(searchTerm) ||
                    t.Subcategory.SubcategoryName.Contains(searchTerm)
                );
            }

            var tickets = await ticketsQuery.ToListAsync();

            ViewBag.Priorities = await _context.Priorities.ToListAsync();
            ViewBag.Technicians = await _context.Technicians.ToListAsync();
            ViewData["CurrentFilter"] = searchTerm;

            return View(tickets);
        }






        // Shows unassigned tickets relevant to this technician’s group
        public async Task<IActionResult> UnassignedTickets(string searchTerm)
        {
            var currentUserId = _userManager.GetUserId(User); // This is ApplicationUser.Id

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == currentUserId);

            if (technician == null)
            {
                return NotFound("Technician not found.");
            }

            var subcategoryIds = await _context.Subcategories
                .Where(s => s.TechnicianGroupId == technician.TechnicianGroupId)
                .Select(s => s.Id)
                .ToListAsync();

            var twoMinutesAgo = DateTime.Now.AddMinutes(-2);

            // Load unassigned, high-priority tickets that are older than 2 minutes and not escalated
            var escalatedTickets = await _context.Tickets
                .Where(t => t.AssignedTechnicianId == null &&
                            t.CreatedAt <= twoMinutesAgo &&
                            t.Priority.PriorityName.ToLower() == "high" &&
                            !t.IsAutoEscalated)
                .Include(t => t.Priority)
                .Include(t => t.Subcategory)
                    .ThenInclude(sc => sc.TechnicianGroup)
                        .ThenInclude(tg => tg.SeniorTechnician)
                .ToListAsync();


            // Group tickets by Senior Technician
            var groupedTickets = escalatedTickets
                .Where(t => t.Subcategory?.TechnicianGroup?.SeniorTechnician != null)
                .GroupBy(t => t.Subcategory.TechnicianGroup.SeniorTechnician);

            foreach (var group in groupedTickets)
            {
                var senior = group.Key;

                var message = new StringBuilder();
                message.AppendLine($"Dear {senior.FullName},");
                message.AppendLine("<br/><br/>The following high-priority ticket(s) have not been attended for over an hour:");

                foreach (var ticket in group)
                {
                    message.AppendLine($"<br/>- Ticket {ticket.TicketNumber}, Created: {ticket.CreatedAt:g}");
                }

                message.AppendLine("<br/><br/>Please take necessary action.");
                message.AppendLine("<br/><br/>Regards,<br/>Ticketing System");

                await _emailSender.SendEmailAsync(senior.Email, "Escalation: Unassigned High-Priority Tickets", message.ToString());

                // Mark these tickets as auto escalated
                foreach (var ticket in group)
                {
                    ticket.IsAutoEscalated = true;
                }
            }

            if (escalatedTickets.Any())
            {
                await _context.SaveChangesAsync();
            }

            // Normal ticket loading
            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Where(t => t.AssignedTechnicianId == null && subcategoryIds.Contains(t.SubcategoryId))
                .Include(t => t.Subcategory)
                .Include(t => t.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Port);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.TicketNumber.Contains(searchTerm) ||
                    t.RequesterName.Contains(searchTerm) ||
                    t.Email.Contains(searchTerm) ||
                    t.Department.DepartmentName.Contains(searchTerm) ||
                    t.Subcategory.SubcategoryName.Contains(searchTerm)
                );
            }

            var tickets = await ticketsQuery.ToListAsync();

            ViewData["CurrentFilter"] = searchTerm;

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
            var currentUserId = _userManager.GetUserId(User); // ApplicationUser.Id
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == currentUserId);

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

            // 📧 Send email to requester
            var subject = $"Ticket {ticket.TicketNumber} Assigned to Technician";
            var message = $@"
<p>Dear {ticket.RequesterName},</p>
<p>Your ticket <strong>{ticket.TicketNumber}</strong> has been assigned to <strong>{technician.FullName}</strong> and is now under resolution.</p>
<p>You will be notified once the issue has been resolved.</p>
<p>Regards,<br/>IT HelpDesk</p>";

            await _emailSender.SendEmailAsync(ticket.Email, subject, message);

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

                // Save uploaded file if exists
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

                // ✅ Assign technician by UserId instead of email
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var technician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.UserId == userId);
                if (technician == null)
                {
                    return Unauthorized();
                }

                ticket.ClosedByTechnicianId = technician.Id;
                _context.Update(ticket);
                await _context.SaveChangesAsync();

                // Prepare acknowledgment email
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

                // ✅ Always send to ticket.Email (if not technician)
                if (ticket.Email != User.Identity.Name)
                {
                    await _emailSender.SendEmailAsync(ticket.Email, subject, body);
                }

                // ✅ If ticket was created by a user, and their email is different from ticket.Email and not the technician
                if (ticket.CreatedBy != null)
                {
                    var user = await _context.Users.FindAsync(ticket.CreatedBy);
                    if (user != null && user.Email != ticket.Email && user.Email != User.Identity.Name)
                    {
                        await _emailSender.SendEmailAsync(user.Email, subject, body);
                    }
                }


                return RedirectToAction("ClosedTickets");
            }

            // Show validation errors if form failed
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
        public async Task<IActionResult> ClosedTickets(string searchTerm)
        {
            var currentUserId = _userManager.GetUserId(User); // ApplicationUser.Id
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == currentUserId);


            if (technician == null)
            {
                return Unauthorized();
            }

            var closedStatusIds = await _context.Status
                .Where(s => s.StatusName == "Closed" || s.StatusName == "Awaiting Acknowledgment")
                .Select(s => s.Id)
                .ToListAsync();

            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.AssignedTechnician)
                .Include(t => t.Subcategory)
                .Where(t => closedStatusIds.Contains(t.StatusId) && t.AssignedTechnicianId == technician.Id);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.TicketNumber.Contains(searchTerm) ||
                    t.RequesterName.Contains(searchTerm) ||
                    t.Email.Contains(searchTerm) ||
                    t.Subcategory.SubcategoryName.Contains(searchTerm)
                );
            }

            var closedTickets = await ticketsQuery.ToListAsync();

            foreach (var ticket in closedTickets)
            {
                bool notificationExists = await _context.Notifications.AnyAsync(n =>
                    n.TicketId == ticket.Id &&
                    n.UserId == ticket.CreatedBy &&
                    n.Message.Contains("has been closed")
                );

                if (!notificationExists)
                {
                    await _notificationService.CreateUserNotification(
                        ticket.CreatedBy,
                        $"Your ticket #{ticket.TicketNumber} has been closed",
                        ticket.Id
                    );
                }
            }


            ViewData["CurrentFilter"] = searchTerm;

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
            var ticket = await _context.Tickets
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.Id == TicketId);
            if (ticket == null) return NotFound();

            // ✅ Get current (reassigning) technician using UserId instead of Email
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var currentTechnician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == userId);
            if (currentTechnician == null) return Unauthorized();


            // Get the new assigned technician
            var newTechnician = await _context.Technicians.FindAsync(TechnicianId);
            if (newTechnician == null)
            {
                ModelState.AddModelError(string.Empty, "Selected technician not found.");
                return RedirectToAction("MyTickets");
            }

            // Update ticket assignment
            ticket.AssignedTechnicianId = TechnicianId;
            ticket.ReassignReason = ReassignReason;
            ticket.StatusChangedAt = DateTime.Now;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            // Send email notification to new technician
            var subject = $"Ticket {ticket.TicketNumber} Reassigned to You";
            var body = $@"
        <p>Dear {newTechnician.FullName},</p>
        <p>The ticket <strong>{ticket.TicketNumber}</strong> has been reassigned to you by <strong>{currentTechnician.FullName}</strong>.</p>
        <p><strong>Reason for reassignment:</strong> {ReassignReason}</p>
        <p>Please log in to the system to review the ticket details.</p>
        <p>Thank you,<br/>IT HelpDesk System</p>";

            await _emailSender.SendEmailAsync(newTechnician.Email, subject, body);

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
            ticket.EscalatedDate = DateTime.UtcNow; 


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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePriority(int ticketId, int priorityId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Priority)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound();

            var newPriority = await _context.Priorities.FindAsync(priorityId);
            if (newPriority == null)
                return BadRequest("Priority not found.");

            ticket.PriorityId = newPriority.Id;

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            // Send email to user
            var subject = $"Ticket {ticket.TicketNumber} Priority Updated";
            var body = $@"
        <p>Dear {ticket.RequesterName},</p>
        <p>The priority of your ticket <strong>{ticket.TicketNumber}</strong> has been updated to <strong>{newPriority.PriorityName}</strong> by a technician.</p>
        <p><strong>Description:</strong> {ticket.Description}</p>
        <p>Thank you,<br/>IT HelpDesk Team</p>";

            await _emailSender.SendEmailAsync(ticket.Email, subject, body);

            TempData["PriorityChanged"] = $"Priority for ticket {ticket.TicketNumber} has been changed to {newPriority.PriorityName}.";
            return RedirectToAction("MyTickets");
        }



    }
}


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
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using ITHelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using ITHelpDesk.Services;
using System.Text;
using ITHelpDesk.Areas.Utility;

namespace ITHelpDesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    { private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;
        private readonly NotificationService _notificationService;

        public TicketsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env, IEmailSender emailSender, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var appUser = user as ApplicationUser;

            if (appUser == null)
            {
                return Unauthorized();
            }

            var ticketsQuery = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Department)
                .Include(t => t.Port)
                .Include(t => t.Priority)
                .Include(t => t.Subcategory);


            // Otherwise, show only tickets created by the logged-in user
            var userTickets = await ticketsQuery
                .Where(t => t.CreatedBy == appUser.Id) // Make sure this field is saved when creating a ticket
                .ToListAsync();

            return View(userTickets);
        }



        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Department)
                .Include(t => t.Port)
                .Include(t => t.Priority)
                .Include(t => t.Subcategory)
                 .Include(t => t.ManuallyAssignedTo)
                  .Include(t => t.ClosedByTechnician)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // GET: Ticket/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var appUser = user as ApplicationUser;

            if (appUser == null)
            {
                return Unauthorized();
            }

            var ticket = new Ticket();

            // Only pre-fill for non-technician roles
            if (!User.IsInRole("Technician") && !User.IsInRole("Technical Support") && !User.IsInRole("Management"))
            {
                ticket.RequesterName = appUser.FullName;
                ticket.Email = appUser.Email;
                ticket.PortId = appUser.PortId;
                ticket.DepartmentId = appUser.DepartmentId;
            }

            // Load technicians for manual assignment dropdown if user is technician/tech support
            if (User.IsInRole("Technician") || User.IsInRole("Technical Support") || User.IsInRole("Management"))
            {
                ViewBag.Technicians = new SelectList(
                    await _context.Technicians.ToListAsync(),
                    "Id",
                    "FullName"
                );
            }

            LoadDropdowns(ticket);
            return View(ticket);
        }




        // POST: Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket, IFormFile? UploadedFile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User) as ApplicationUser;
                if (user == null)
                    return Unauthorized();

                var subcategory = await _context.Subcategories
                    .Include(s => s.TechnicianGroups)
                    .FirstOrDefaultAsync(s => s.Id == ticket.SubcategoryId);

                if (subcategory == null)
                {
                    ModelState.AddModelError("SubcategoryId", "Invalid subcategory selected.");
                    LoadDropdowns(ticket);
                    return View(ticket);
                }

                ticket.TechnicianGroups = subcategory.TechnicianGroups.ToList();

                var isTechOrSupport = await _userManager.IsInRoleAsync(user, "Technician") ||
                                      await _userManager.IsInRoleAsync(user, "TechnicalSupport");

                Technician? assignedTech = null;

                if (isTechOrSupport && ticket.ManuallyAssignedToId.HasValue)
                {
                    ticket.AssignedTechnicianId = ticket.ManuallyAssignedToId.Value;
                    assignedTech = await _context.Technicians.FindAsync(ticket.AssignedTechnicianId);

                    var assignedStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Assigned");
                    ticket.StatusId = assignedStatus?.Id ?? ticket.StatusId;
                }
                else
                {
                    ticket.Mode = null;
                    ticket.ManuallyAssignedToId = null;
                    ticket.EmailToNotify = null;
                    ticket.Organization = null;

                    var defaultStatus = await _context.Status.FirstOrDefaultAsync(s => s.StatusName == "Unassigned");
                    if (defaultStatus == null)
                    {
                        ModelState.AddModelError("", "Default status 'Unassigned' not found.");
                        LoadDropdowns(ticket);
                        return View(ticket);
                    }
                    ticket.StatusId = defaultStatus.Id;
                }

                // Handle File Upload
                if (UploadedFile != null && UploadedFile.Length > 0)
                {
                    var fileName = Path.GetFileName(UploadedFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await UploadedFile.CopyToAsync(stream);

                    ticket.FileName = fileName;
                }

                if (!ModelState.IsValid)
                {
                    LoadDropdowns(ticket);
                    return View(ticket);
                }

                // Metadata
                ticket.CreatedAt = DateTime.Now;
                ticket.CreatedBy = user.Id;

                // Ticket Number Generation
                var lastUserTicketNumber = await _context.Tickets
                    .Where(t => t.CreatedBy == user.Id)
                    .OrderByDescending(t => t.TicketNumber)
                    .Select(t => t.TicketNumber)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrWhiteSpace(lastUserTicketNumber))
                {
                    var match = Regex.Match(lastUserTicketNumber, @"#TKT-(\d+)");
                    if (match.Success)
                        nextNumber = int.Parse(match.Groups[1].Value) + 1;
                }
                ticket.TicketNumber = $"#TKT-{nextNumber:D3}";

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Manual assignment email
                if (isTechOrSupport && ticket.ManuallyAssignedToId.HasValue && !string.IsNullOrWhiteSpace(ticket.EmailToNotify))
                {
                    try
                    {
                        var subject = $"New Ticket Assigned: {ticket.TicketNumber}";
                        var body = $@"
<html>
<body>
    <h3>A support ticket has been manually created and assigned:</h3>
    <ul>
        <li><strong>Ticket Number:</strong> {ticket.TicketNumber}</li>
        <li><strong>Assigned To:</strong> {assignedTech?.FullName ?? "Technician"}</li>
        <li><strong>Description:</strong> {ticket.Description}</li>
        <li><strong>Date:</strong> {ticket.CreatedAt:yyyy/MM/dd HH:mm:ss}</li>
    </ul>
    <p>Regards,<br />IT Helpdesk System</p>
</body>
</html>";

                        await _emailSender.SendEmailAsync(ticket.EmailToNotify, subject, body);
                    }
                    catch (Exception emailEx)
                    {
                        ModelState.AddModelError("", $"Manual email failed: {emailEx.Message}");
                    }
                }


                // Automatic notifications for high-priority tickets
                if (!isTechOrSupport || !ticket.ManuallyAssignedToId.HasValue)
                {
                    ticket = await _context.Tickets
                        .Include(t => t.Priority)
                        .Include(t => t.TechnicianGroups)
                        .FirstOrDefaultAsync(t => t.Id == ticket.Id);

                    var technicianGroupIds = ticket.TechnicianGroups?.Select(g => g.Id).ToList() ?? new();

                    if (ticket.Priority?.PriorityName == "High" && technicianGroupIds.Any() && ticket.PortId > 0)
                    {
                        var techniciansToNotify = await _context.Technicians
                            .Where(t => technicianGroupIds.Contains(t.TechnicianGroupId))
                            .Where(t => t.TechnicianPorts.Any(tp => tp.PortId == ticket.PortId))
                            .ToListAsync();

                        foreach (var tech in techniciansToNotify)
                        {
                            await _notificationService.CreateTechnicianNotification(
                                tech.Id,
                                $"🚨 High priority ticket {ticket.TicketNumber}",
                                ticket.Id);
                        }
                    }

                    // Email notification to matching technicians
                    var matchingTechs = await _context.Technicians
                         .Include(t => t.TechnicianGroup)
                         .Include(t => t.TechnicianPorts)
                         .Where(t => technicianGroupIds.Contains(t.TechnicianGroupId) &&
                                t.TechnicianPorts.Any(tp => tp.PortId == ticket.PortId))
                         .ToListAsync();


                    var subject = $"New Ticket Created: {ticket.TicketNumber}";
                    var emailBody = $@"
                <p>A new support ticket has been submitted:</p>
                        <ul>
                           <li><strong>Ticket Number:</strong> {ticket.TicketNumber}</li>
                           <li><strong>Description:</strong> {ticket.Description}</li>
                           <li><strong>Priority:</strong> {ticket.Priority?.PriorityName ?? "N/A"}</li>
                           <li><strong>Submitted By:</strong> {ticket.RequesterName}</li>
                           <li><strong>Date:</strong> {ticket.CreatedAt:yyyy/MM/dd HH:mm:ss}</li>
                        </ul>  
                        <p>Please log in to the system to view and assign the ticket.</p>
                        <p> Regards,<br/> IT Helpdesk System</p> ";

                    foreach (var tech in matchingTechs)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(tech.Email))
                            {
                                await _emailSender.SendEmailAsync(tech.Email, subject, emailBody);
                            }
                        }
                        catch (Exception emailEx)
                        {
                            ModelState.AddModelError("", $"Failed to email {tech.Email}: {emailEx.Message}");
                        }
                    }
                }

                TempData["SuccessMessage"] = "Ticket successfully created.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                    ModelState.AddModelError("", $"Inner Exception: {ex.InnerException.Message}");

                LoadDropdowns(ticket);
                return View(ticket);
            }
        }






        // Updated LoadDropdowns to preserve selected values
        private void LoadDropdowns(Ticket ticket = null)
        {
            ViewBag.PortId = new SelectList(_context.Ports, "Id", "PortName", ticket?.PortId);
            ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "DepartmentName", ticket?.DepartmentId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "CategoryName", ticket?.CategoryId);
            ViewBag.SubcategoryId = new SelectList(_context.Subcategories, "Id", "SubcategoryName", ticket?.SubcategoryId);
            ViewBag.PriorityId = new SelectList(_context.Priorities, "Id", "PriorityName", ticket?.PriorityId);

            // Add RequestMode enum to ViewBag
            ViewBag.RequestModes = Enum.GetValues(typeof(RequestMode))
                .Cast<RequestMode>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                });
        }

        [HttpGet]
        public JsonResult GetSubcategoriesByCategory(int categoryId)
        {
            var subcategories = _context.Subcategories
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new {
                    id = s.Id,
                    subcategoryName = s.SubcategoryName
                })
                .ToList();

            return Json(subcategories);
        }


        public async Task<IActionResult> GetTechniciansBySubcategory(int subcategoryId)
        {
            // Load Subcategory with its TechnicianGroups
            var subcategory = await _context.Subcategories
                .Include(s => s.TechnicianGroups)
                .FirstOrDefaultAsync(s => s.Id == subcategoryId);

            if (subcategory == null || subcategory.TechnicianGroups == null || !subcategory.TechnicianGroups.Any())
            {
                return Json(new List<object>());
            }

            // Get all TechnicianGroupIds related to this subcategory
            var technicianGroupIds = subcategory.TechnicianGroups.Select(g => g.Id).ToList();

            // Get all technicians that belong to any of those groups
            var technicians = await _context.Technicians
                .Where(t => technicianGroupIds.Contains(t.TechnicianGroupId))
                .Select(t => new { id = t.Id, name = t.FullName })
                .ToListAsync();

            return Json(technicians);
        }
  

        [HttpGet]
        public JsonResult GetTechnicianEmail(int technicianId)
        {
            var technician = _context.Technicians.FirstOrDefault(t => t.Id == technicianId);
            if (technician != null)
            {
                return Json(new { email = technician.Email });
            }
            return Json(new { email = "" });
        }



        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", ticket.CategoryId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DepartmentName", ticket.DepartmentId);
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", ticket.PortId);
            ViewData["PriorityId"] = new SelectList(_context.Priorities, "Id", "PriorityName", ticket.PriorityId);
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "SubcategoryName", ticket.SubcategoryId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RequesterName,Email,PortId,DepartmentId,CategoryId,SubcategoryId,PriorityId,Description,UploadedFilePath,CreatedAt")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", ticket.CategoryId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DepartmentName", ticket.DepartmentId);
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", ticket.PortId);
            ViewData["PriorityId"] = new SelectList(_context.Priorities, "Id", "PriorityName", ticket.PriorityId);
            ViewData["SubcategoryId"] = new SelectList(_context.Subcategories, "Id", "SubcategoryName", ticket.SubcategoryId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Department)
                .Include(t => t.Port)
                .Include(t => t.Priority)
                .Include(t => t.Subcategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }


        public async Task<IActionResult> PortTickets(string month, int? portId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isManagement = await _userManager.IsInRoleAsync(user, "Management");

            IQueryable<Ticket> ticketsQuery;

            if (isManagement)
            {
                var ports = await _context.Ports.OrderBy(p => p.PortName).ToListAsync();
                ViewBag.Ports = ports.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.PortName,
                    Selected = p.Id == portId
                }).ToList();

                ticketsQuery = _context.Tickets
                    .Include(t => t.Status)
                    .Include(t => t.Port)
                    .Include(t => t.Category)
                    .Include(t => t.Subcategory)
                    .Include(t => t.Priority)
                    .Include(t => t.AssignedTechnician)
                    .Include(t => t.Department);

                if (portId.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.PortId == portId.Value);
                    ViewBag.SelectedPortId = portId;
                }
                else
                {
                    ViewBag.SelectedPortId = null;
                }
            }
            else // Technician
            {
                var userId = _userManager.GetUserId(User);
                var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == userId);
                if (technician == null)
                    return View(new List<Ticket>());

                var assignedPortIds = await _context.TechnicianPorts
                    .Where(tp => tp.TechnicianId == technician.Id)
                    .Select(tp => tp.PortId)
                    .ToListAsync();

                if (!assignedPortIds.Any())
                    return View(new List<Ticket>());

                ticketsQuery = _context.Tickets
                    .Include(t => t.Status)
                    .Include(t => t.Port)
                    .Include(t => t.Category)
                    .Include(t => t.Subcategory)
                    .Include(t => t.Priority)
                    .Include(t => t.AssignedTechnician)
                    .Include(t => t.Department)
                    .Where(t => assignedPortIds.Contains(t.PortId));

                ViewBag.Ports = null;
                ViewBag.SelectedPortId = assignedPortIds.First(); // default to their first port
            }

            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out var selectedMonth))
            {
                ticketsQuery = ticketsQuery.Where(t => t.CreatedAt.Month == selectedMonth.Month && t.CreatedAt.Year == selectedMonth.Year);
                ViewBag.SelectedMonth = month;
                ViewBag.SelectedMonthName = selectedMonth.ToString("MMMM yyyy");
            }
            else
            {
                ViewBag.SelectedMonth = "";
                ViewBag.SelectedMonthName = "All";
            }

            var tickets = await ticketsQuery.ToListAsync();
            return View(tickets);
        }


        [HttpPost]
        public async Task<IActionResult> ExportTickets(string month, int? portId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isManagement = await _userManager.IsInRoleAsync(user, "Management");

            DateTime selectedMonth = DateTime.MinValue;
            bool monthFilterApplied = !string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out selectedMonth);

            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Port)
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .Include(t => t.Priority)
                .Include(t => t.AssignedTechnician)
                .Include(t => t.Department);

            if (isManagement)
            {
                if (portId.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.PortId == portId.Value);
                }
            }
            else // Technician
            {
                var userId = _userManager.GetUserId(User);
                var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == userId);
                if (technician == null)
                    return BadRequest("Technician not found.");

                var assignedPortIds = await _context.TechnicianPorts
                    .Where(tp => tp.TechnicianId == technician.Id)
                    .Select(tp => tp.PortId)
                    .ToListAsync();

                if (!assignedPortIds.Any())
                    return BadRequest("No assigned ports.");

                ticketsQuery = ticketsQuery.Where(t => assignedPortIds.Contains(t.PortId));
            }

            if (monthFilterApplied)
            {
                ticketsQuery = ticketsQuery
                    .Where(t => t.CreatedAt.Month == selectedMonth.Month && t.CreatedAt.Year == selectedMonth.Year);
            }

            var tickets = await ticketsQuery.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Ticket ID,Requester Name,Department,Port,Priority,Category,Subcategory,Status,Assigned Technician,Created Date");

            foreach (var t in tickets)
            {
                csv.AppendLine($"{t.TicketNumber}," +
                               $"{t.RequesterName}," +
                               $"{t.Department?.DepartmentName}," +
                               $"{t.Port?.PortName}," +
                               $"{t.Priority?.PriorityName}," +
                               $"{t.Category?.CategoryName}," +
                               $"{t.Subcategory?.SubcategoryName}," +
                               $"{t.Status?.StatusName}," +
                               $"{t.AssignedTechnician?.FullName}," +
                               $"{t.CreatedAt:yyyy-MM-dd}");
            }

            var fileName = $"PortTickets_{(monthFilterApplied ? month : "All")}_{(portId.HasValue ? "Port" + portId : "TechnicianAssigned")}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }

    }
}
 
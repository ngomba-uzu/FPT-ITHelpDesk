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

namespace ITHelpDesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    { private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;

        public TicketsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _emailSender = emailSender;
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

           /* // If user is Admin or Technician, show all tickets
            if (await _userManager.IsInRoleAsync(appUser, "Admin") || await _userManager.IsInRoleAsync(appUser, "Technician"))
            {
                return View(await ticketsQuery.ToListAsync());
            }
*/
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

            var ticket = new Ticket
            {
                RequesterName = appUser.FullName,
                Email = appUser.Email,
                PortId = appUser.PortId,
                DepartmentId = appUser.DepartmentId
            };

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
                var user = await _userManager.GetUserAsync(User);
                var appUser = user as ApplicationUser;

                if (appUser == null)
                {
                    return Unauthorized();
                }

                // ✅ Get the TechnicianGroupId from the selected Subcategory
                var subcategory = await _context.Subcategories
                                        .FirstOrDefaultAsync(s => s.Id == ticket.SubcategoryId);

                if (subcategory == null)
                {
                    ModelState.AddModelError("SubcategoryId", "Invalid subcategory selected.");
                    LoadDropdowns(ticket);
                    return View(ticket);
                }

                ticket.TechnicianGroupId = subcategory.TechnicianGroupId;

                if (UploadedFile != null && UploadedFile.Length > 0)
                {
                    var fileName = Path.GetFileName(UploadedFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadedFile.CopyToAsync(stream);
                    }

                    ticket.FileName = fileName;
                }

                if (!ModelState.IsValid)
                {
                    LoadDropdowns(ticket);
                    return View(ticket);
                }

                ticket.CreatedAt = DateTime.Now;
                ticket.RequesterName = appUser.FullName;
                ticket.Email = appUser.Email;
                ticket.PortId = appUser.PortId;
                ticket.DepartmentId = appUser.DepartmentId;
                ticket.CreatedBy = appUser.Id;

                var defaultStatus = await _context.Status
                 .FirstOrDefaultAsync(s => s.StatusName == "Unassigned");

                if (defaultStatus == null)
                {
                    ModelState.AddModelError(string.Empty, "Default status 'Unassigned' not found. Please add it to the database.");
                    LoadDropdowns(ticket);
                    return View(ticket);
                }

                ticket.StatusId = defaultStatus.Id;


                // ✅ Generate Ticket Number
                var lastTicketNumber = await _context.Tickets
                    .Where(t => t.TicketNumber.StartsWith("#TKT-"))
                    .OrderByDescending(t => t.TicketNumber)
                    .Select(t => t.TicketNumber)
                    .FirstOrDefaultAsync();

                int newTicketSequence = 1;

                if (!string.IsNullOrEmpty(lastTicketNumber))
                {
                    var match = Regex.Match(lastTicketNumber, @"#TKT-(\d+)");
                    if (match.Success)
                    {
                        newTicketSequence = int.Parse(match.Groups[1].Value) + 1;
                    }
                }

                ticket.TicketNumber = $"#TKT-{newTicketSequence.ToString("D3")}";


                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();  // Save changes to the database
                
                ticket = await _context.Tickets
                  .Include(t => t.Priority)
                  .FirstOrDefaultAsync(t => t.Id == ticket.Id);


                // ✅ Send email to all technicians in the TechnicianGroup
                var technicianGroup = await _context.TechnicianGroups
                    .Include(g => g.Technicians)
                    .FirstOrDefaultAsync(g => g.Id == ticket.TechnicianGroupId);

                if (technicianGroup != null && technicianGroup.Technicians.Any())
                {

                    var subject = $"New Ticket Created: {ticket.TicketNumber}";
    var message = $@"
        <p>A new support ticket has been submitted:</p>
        <ul>
            <li><strong>Ticket Number:</strong> {ticket.TicketNumber}</li>
            <li><strong>Description:</strong> {ticket.Description}</li>
            <li><strong>Priority:</strong> {ticket.Priority?.PriorityName ?? "N/A"}</li>
            <li><strong>Submitted By:</strong> {ticket.RequesterName}</li>
            <li><strong>Date:</strong> {ticket.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss")}</li>
        </ul>
        <p>Please log in to the system to view and assign the ticket.</p>
    ";

                    foreach (var technician in technicianGroup.Technicians)
                    {
                        if (!string.IsNullOrEmpty(technician.Email))
                        {
                            await _emailSender.SendEmailAsync(technician.Email, subject, message);
                        }
                    }
                }




                // ✅ Show success message
                TempData["SuccessMessage"] = "Ticket successfully created.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, "Inner Exception: " + ex.InnerException.Message);
                }

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

    }
}

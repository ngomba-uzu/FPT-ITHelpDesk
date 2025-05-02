using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelpDesk.Data;
using ITHelpDesk.Models;

namespace ITHelpDesk.Controllers
{
    public class SeniorTechniciansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeniorTechniciansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SeniorTechnicians
        public async Task<IActionResult> Index()
        {
            return View(await _context.SeniorTechnicians.ToListAsync());
        }

        // GET: SeniorTechnicians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seniorTechnician = await _context.SeniorTechnicians
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seniorTechnician == null)
            {
                return NotFound();
            }

            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SeniorTechnicians/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Email")] SeniorTechnician seniorTechnician)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seniorTechnician);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seniorTechnician = await _context.SeniorTechnicians.FindAsync(id);
            if (seniorTechnician == null)
            {
                return NotFound();
            }
            return View(seniorTechnician);
        }

        // POST: SeniorTechnicians/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email")] SeniorTechnician seniorTechnician)
        {
            if (id != seniorTechnician.Id)
            {
                return NotFound();
            }

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
            return View(seniorTechnician);
        }

        // GET: SeniorTechnicians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seniorTechnician = await _context.SeniorTechnicians
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seniorTechnician == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeniorTechnicianExists(int id)
        {
            return _context.SeniorTechnicians.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ReviewEscalatedTicket(int id)
        {
            var ticket = await _context.Tickets
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
            var ticket = await _context.Tickets.FindAsync(TicketId);
            if (ticket == null) return NotFound();

            ticket.SeniorTechnicianResponse = Response;
            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return Content("Response submitted successfully. The technician will now handle it.");
        }

    }
}

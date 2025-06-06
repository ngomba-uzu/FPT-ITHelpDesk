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
    public class TechnicianGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicianGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TechnicianGroups
        public async Task<IActionResult> Index()
        {
            var groups = await _context.TechnicianGroups.ToListAsync();
            return View(groups);
        }

        // GET: TechnicianGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var technicianGroup = await _context.TechnicianGroups
                .FirstOrDefaultAsync(m => m.Id == id);

            if (technicianGroup == null)
                return NotFound();

            return View(technicianGroup);
        }

        // GET: TechnicianGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TechnicianGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupName")] TechnicianGroup technicianGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(technicianGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(technicianGroup);
        }

        // GET: TechnicianGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var technicianGroup = await _context.TechnicianGroups.FindAsync(id);
            if (technicianGroup == null)
                return NotFound();

            return View(technicianGroup);
        }

        // POST: TechnicianGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GroupName")] TechnicianGroup technicianGroup)
        {
            if (id != technicianGroup.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(technicianGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianGroupExists(technicianGroup.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(technicianGroup);
        }

        // GET: TechnicianGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var technicianGroup = await _context.TechnicianGroups
                .FirstOrDefaultAsync(m => m.Id == id);

            if (technicianGroup == null)
                return NotFound();

            return View(technicianGroup);
        }

        // POST: TechnicianGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technicianGroup = await _context.TechnicianGroups.FindAsync(id);
            if (technicianGroup != null)
            {
                _context.TechnicianGroups.Remove(technicianGroup);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TechnicianGroupExists(int id)
        {
            return _context.TechnicianGroups.Any(e => e.Id == id);
        }
    }

}


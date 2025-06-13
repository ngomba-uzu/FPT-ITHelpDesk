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
    public class SubcategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubcategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Subcategories
        public async Task<IActionResult> Index()
        {
            var subcategories = _context.Subcategories
                .Include(s => s.Category)
                .Include(s => s.TechnicianGroups);
            return View(await subcategories.ToListAsync());
        }

        // GET: Subcategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subcategory = await _context.Subcategories
                .Include(s => s.Category)
                .Include(s => s.TechnicianGroups)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subcategory == null) return NotFound();

            return View(subcategory);
        }

        // GET: Subcategories/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewData["TechnicianGroups"] = new MultiSelectList(_context.TechnicianGroups, "Id", "GroupName");
            return View();
        }

        // POST: Subcategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subcategory subcategory, int[] selectedTechnicianGroups)
        {
            if (ModelState.IsValid)
            {
                if (selectedTechnicianGroups != null && selectedTechnicianGroups.Length > 0)
                {
                    var technicianGroups = await _context.TechnicianGroups
                        .Where(g => selectedTechnicianGroups.Contains(g.Id))
                        .ToListAsync();

                    foreach (var group in technicianGroups)
                    {
                        subcategory.TechnicianGroups.Add(group);
                    }
                }

                _context.Add(subcategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Re-populate dropdowns if model state is invalid
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", subcategory.CategoryId);
            ViewData["TechnicianGroups"] = new MultiSelectList(_context.TechnicianGroups, "Id", "GroupName", selectedTechnicianGroups);
            return View(subcategory);
        }


        // GET: Subcategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subcategory = await _context.Subcategories
                .Include(s => s.TechnicianGroups)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subcategory == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", subcategory.CategoryId);
            ViewData["TechnicianGroups"] = new MultiSelectList(_context.TechnicianGroups, "Id", "GroupName", subcategory.TechnicianGroups.Select(t => t.Id));
            return View(subcategory);
        }

        // POST: Subcategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subcategory subcategory, int[] selectedTechnicianGroups)
        {
            if (id != subcategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSubcategory = await _context.Subcategories
                        .Include(s => s.TechnicianGroups)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (existingSubcategory == null) return NotFound();

                    // Update simple properties
                    existingSubcategory.SubcategoryName = subcategory.SubcategoryName;
                    existingSubcategory.CategoryId = subcategory.CategoryId;

                    // Update technician group associations
                    existingSubcategory.TechnicianGroups.Clear();

                    foreach (var groupId in selectedTechnicianGroups)
                    {
                        var technicianGroup = await _context.TechnicianGroups.FindAsync(groupId);
                        if (technicianGroup != null)
                        {
                            existingSubcategory.TechnicianGroups.Add(technicianGroup);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubcategoryExists(subcategory.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", subcategory.CategoryId);
            ViewData["TechnicianGroups"] = new MultiSelectList(_context.TechnicianGroups, "Id", "GroupName", selectedTechnicianGroups);
            return View(subcategory);
        }

        // GET: Subcategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subcategory = await _context.Subcategories
                .Include(s => s.Category)
                .Include(s => s.TechnicianGroups)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subcategory == null) return NotFound();

            return View(subcategory);
        }

        // POST: Subcategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcategory = await _context.Subcategories
                .Include(s => s.TechnicianGroups)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subcategory != null)
            {
                // Clear many-to-many relationship first
                subcategory.TechnicianGroups.Clear();
                _context.Subcategories.Remove(subcategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubcategoryExists(int id)
        {
            return _context.Subcategories.Any(e => e.Id == id);
        }
    }

}


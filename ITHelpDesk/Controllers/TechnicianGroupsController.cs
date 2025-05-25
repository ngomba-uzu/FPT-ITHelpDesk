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
            var groups = await _context.TechnicianGroups
                .Include(g => g.Technicians)
                   .Include(g => g.SeniorTechnician) // ✅ Include Senior Technician 
                .ToListAsync();

            return View(groups);
        }


        // GET: TechnicianGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianGroup = await _context.TechnicianGroups
                .Include(g => g.Technicians) // 👈 Include related technicians
                  .Include(g => g.SeniorTechnician) // 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (technicianGroup == null)
            {
                return NotFound();
            }

            return View(technicianGroup);
        }

        // GET: TechnicianGroups/Create
        public IActionResult Create()
        {
            var technicianGroup = new TechnicianGroup
            {
                Technicians = new List<Technician>(), // optional, just for clarity
                SelectedTechnicianIds = new List<int>()
            };
            ViewBag.SeniorTechnicianId = new SelectList(_context.SeniorTechnicians, "Id", "FullName");
            ViewBag.TechnicianList = new MultiSelectList(_context.Technicians, "Id", "FullName");

            return View(technicianGroup);
        }


        // POST: TechnicianGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechnicianGroup technicianGroup)
        {
            if (ModelState.IsValid)
            {
                // Get the selected technicians
                if (technicianGroup.SelectedTechnicianIds != null && technicianGroup.SelectedTechnicianIds.Any())
                {
                    technicianGroup.Technicians = await _context.Technicians
                        .Where(t => technicianGroup.SelectedTechnicianIds.Contains(t.Id))
                        .ToListAsync();
                }

                _context.Add(technicianGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate technician list if model state is invalid
            ViewBag.AllTechnicians = _context.Technicians.ToList();
            ViewBag.SeniorTechnicianId = new SelectList(_context.SeniorTechnicians, "Id", "FullName", technicianGroup.SeniorTechnicianId);
            return View(technicianGroup);
        }


        // GET: TechnicianGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianGroup = await _context.TechnicianGroups
      .Include(g => g.Technicians)
      .FirstOrDefaultAsync(g => g.Id == id);

            if (technicianGroup == null)
            {
                return NotFound();
            }

            // Populate SelectedTechnicianIds for the view
            technicianGroup.SelectedTechnicianIds = technicianGroup.Technicians.Select(t => t.Id).ToList();

            // Send all available technicians to the view
            ViewBag.AllTechnicians = _context.Technicians.ToList();
            ViewBag.SeniorTechnicianId = new SelectList(_context.SeniorTechnicians, "Id", "FullName", technicianGroup.SeniorTechnicianId);

            return View(technicianGroup);
        }

        // POST: TechnicianGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TechnicianGroup technicianGroup)
        {
            if (id != technicianGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var groupToUpdate = await _context.TechnicianGroups
                        .Include(g => g.Technicians)
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (groupToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Update the group name
                    groupToUpdate.GroupName = technicianGroup.GroupName;
                    groupToUpdate.SeniorTechnicianId = technicianGroup.SeniorTechnicianId; // ✅ Add this

                    // Update technicians
                    groupToUpdate.Technicians.Clear();
                    if (technicianGroup.SelectedTechnicianIds != null && technicianGroup.SelectedTechnicianIds.Any())
                    {
                        var selectedTechnicians = await _context.Technicians
                            .Where(t => technicianGroup.SelectedTechnicianIds.Contains(t.Id))
                            .ToListAsync();

                        foreach (var tech in selectedTechnicians)
                        {
                            groupToUpdate.Technicians.Add(tech);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianGroupExists(technicianGroup.Id))
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

            ViewBag.TechnicianList = new MultiSelectList(_context.Technicians, "Id", "FullName", technicianGroup.SelectedTechnicianIds);
            ViewBag.SeniorTechnicianId = new SelectList(_context.SeniorTechnicians, "Id", "FullName", technicianGroup.SeniorTechnicianId);
            return View(technicianGroup);
        }


        // GET: TechnicianGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianGroup = await _context.TechnicianGroups
                .Include(g => g.Technicians)
                 .Include(g => g.SeniorTechnician)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (technicianGroup == null)
            {
                return NotFound();
            }

            return View(technicianGroup);
        }


        // POST: TechnicianGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technicianGroup = await _context.TechnicianGroups
                .Include(g => g.Technicians) // 👈 Include the technicians
                .FirstOrDefaultAsync(g => g.Id == id);

            if (technicianGroup != null)
            {
                // 👇 Clear technician assignments to avoid foreign key issues
                technicianGroup.Technicians.Clear();

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

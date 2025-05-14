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
    public class TechnicianPortsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicianPortsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TechnicianPorts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TechnicianPorts.Include(t => t.Port).Include(t => t.Technician);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TechnicianPorts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianPort = await _context.TechnicianPorts
                .Include(t => t.Port)
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technicianPort == null)
            {
                return NotFound();
            }

            return View(technicianPort);
        }

        // GET: TechnicianPorts/Create
        public IActionResult Create()
        {
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName");
            ViewData["TechnicianId"] = new SelectList(_context.Technicians, "Id", "FullName");
            return View();
        }

        // POST: TechnicianPorts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TechnicianId,PortId")] TechnicianPort technicianPort)
        {
            if (ModelState.IsValid)
            {
                _context.Add(technicianPort);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", technicianPort.PortId);
            ViewData["TechnicianId"] = new SelectList(_context.Technicians, "Id", "FullName", technicianPort.TechnicianId);
            return View(technicianPort);
        }

        // GET: TechnicianPorts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianPort = await _context.TechnicianPorts.FindAsync(id);
            if (technicianPort == null)
            {
                return NotFound();
            }
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", technicianPort.PortId);
            ViewData["TechnicianId"] = new SelectList(_context.Technicians, "Id", "FullName", technicianPort.TechnicianId);
            return View(technicianPort);
        }

        // POST: TechnicianPorts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TechnicianId,PortId")] TechnicianPort technicianPort)
        {
            if (id != technicianPort.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(technicianPort);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianPortExists(technicianPort.Id))
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
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", technicianPort.PortId);
            ViewData["TechnicianId"] = new SelectList(_context.Technicians, "Id", "FullName", technicianPort.TechnicianId);
            return View(technicianPort);
        }

        // GET: TechnicianPorts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technicianPort = await _context.TechnicianPorts
                .Include(t => t.Port)
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technicianPort == null)
            {
                return NotFound();
            }

            return View(technicianPort);
        }

        // POST: TechnicianPorts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technicianPort = await _context.TechnicianPorts.FindAsync(id);
            if (technicianPort != null)
            {
                _context.TechnicianPorts.Remove(technicianPort);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TechnicianPortExists(int id)
        {
            return _context.TechnicianPorts.Any(e => e.Id == id);
        }
    }
}

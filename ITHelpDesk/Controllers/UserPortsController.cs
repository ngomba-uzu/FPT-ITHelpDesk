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
    public class UserPortsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserPortsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserPorts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserPorts.Include(u => u.Port).Include(u => u.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UserPorts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userPorts = await _context.UserPorts
                .Include(u => u.Port)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userPorts == null)
            {
                return NotFound();
            }

            return View(userPorts);
        }

        // GET: UserPorts/Create
        public IActionResult Create()
        {
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: UserPorts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,PortId")] UserPorts userPorts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userPorts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", userPorts.PortId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", userPorts.UserId);
            return View(userPorts);
        }

        // GET: UserPorts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userPorts = await _context.UserPorts.FindAsync(id);
            if (userPorts == null)
            {
                return NotFound();
            }
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", userPorts.PortId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", userPorts.UserId);
            return View(userPorts);
        }

        // POST: UserPorts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,PortId")] UserPorts userPorts)
        {
            if (id != userPorts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userPorts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserPortsExists(userPorts.Id))
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
            ViewData["PortId"] = new SelectList(_context.Ports, "Id", "PortName", userPorts.PortId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", userPorts.UserId);
            return View(userPorts);
        }

        // GET: UserPorts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userPorts = await _context.UserPorts
                .Include(u => u.Port)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userPorts == null)
            {
                return NotFound();
            }

            return View(userPorts);
        }

        // POST: UserPorts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userPorts = await _context.UserPorts.FindAsync(id);
            if (userPorts != null)
            {
                _context.UserPorts.Remove(userPorts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserPortsExists(int id)
        {
            return _context.UserPorts.Any(e => e.Id == id);
        }
    }
}

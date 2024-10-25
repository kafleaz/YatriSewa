using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YatriSewa.Models;

namespace YatriSewa.Controllers
{
    public class BusDriversController : Controller
    {
        private readonly ApplicationContext _context;

        public BusDriversController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: BusDrivers
        public async Task<IActionResult> Index()
        {
            var applicationContext = _context.Driver_Table.Include(b => b.User);
            return View(await applicationContext.ToListAsync());
        }

        // GET: BusDrivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busDriver = await _context.Driver_Table
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (busDriver == null)
            {
                return NotFound();
            }

            return View(busDriver);
        }

        // GET: BusDrivers/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name");
            return View();
        }

        // POST: BusDrivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DriverId,DriverName,LicenseNumber,PhoneNumber,Address,DateOfBirth,LicensePhotoPath,IsAvailable,UserId")] BusDriver busDriver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(busDriver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", busDriver.UserId);
            return View(busDriver);
        }

        // GET: BusDrivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busDriver = await _context.Driver_Table.FindAsync(id);
            if (busDriver == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", busDriver.UserId);
            return View(busDriver);
        }

        // POST: BusDrivers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DriverId,DriverName,LicenseNumber,PhoneNumber,Address,DateOfBirth,LicensePhotoPath,IsAvailable,UserId")] BusDriver busDriver)
        {
            if (id != busDriver.DriverId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(busDriver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusDriverExists(busDriver.DriverId))
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
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", busDriver.UserId);
            return View(busDriver);
        }

        // GET: BusDrivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busDriver = await _context.Driver_Table
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (busDriver == null)
            {
                return NotFound();
            }

            return View(busDriver);
        }

        // POST: BusDrivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var busDriver = await _context.Driver_Table.FindAsync(id);
            if (busDriver != null)
            {
                _context.Driver_Table.Remove(busDriver);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusDriverExists(int id)
        {
            return _context.Driver_Table.Any(e => e.DriverId == id);
        }
    }
}

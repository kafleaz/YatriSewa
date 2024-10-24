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
    public class BusesController : Controller
    {
        private readonly ApplicationContext _context;

        public BusesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Buses
        public async Task<IActionResult> Index()
        {
            var applicationContext = _context.Bus_Table.Include(b => b.BusCompany).Include(b => b.BusDriver).Include(b => b.Route);
            return View(await applicationContext.ToListAsync());
        }

        // GET: Buses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Bus_Table
                .Include(b => b.BusCompany)
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.BusId == id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        // GET: Buses/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName");
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address");
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation");
            return View();
        }

        // POST: Buses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusId,BusName,BusNumber,Description,SeatCapacity,Price,CompanyId,RouteId,DriverId")] Bus bus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", bus.CompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", bus.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", bus.RouteId);
            return View(bus);
        }

        // GET: Buses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Bus_Table.FindAsync(id);
            if (bus == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", bus.CompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", bus.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", bus.RouteId);
            return View(bus);
        }

        // POST: Buses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusId,BusName,BusNumber,Description,SeatCapacity,Price,CompanyId,RouteId,DriverId")] Bus bus)
        {
            if (id != bus.BusId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusExists(bus.BusId))
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
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", bus.CompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", bus.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", bus.RouteId);
            return View(bus);
        }

        // GET: Buses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Bus_Table
                .Include(b => b.BusCompany)
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.BusId == id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        // POST: Buses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bus = await _context.Bus_Table.FindAsync(id);
            if (bus != null)
            {
                _context.Bus_Table.Remove(bus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusExists(int id)
        {
            return _context.Bus_Table.Any(e => e.BusId == id);
        }
    }
}

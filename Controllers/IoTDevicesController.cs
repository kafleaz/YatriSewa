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
    public class IoTDevicesController : Controller
    {
        private readonly ApplicationContext _context;

        public IoTDevicesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: IoTDevices
        public async Task<IActionResult> Index()
        {
            var applicationContext = _context.IoTDevices.Include(i => i.Bus);
            return View(await applicationContext.ToListAsync());
        }

        // GET: IoTDevices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ioTDevice = await _context.IoTDevices
                .Include(i => i.Bus)
                .FirstOrDefaultAsync(m => m.DeviceId == id);
            if (ioTDevice == null)
            {
                return NotFound();
            }

            return View(ioTDevice);
        }

        // GET: IoTDevices/Create
        public IActionResult Create()
        {
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName");
            return View();
        }

        // POST: IoTDevices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeviceId,DeviceName,DeviceIdentifier,Latitude,Longitude,Speed,BusId,LastUpdated")] IoTDevice ioTDevice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ioTDevice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", ioTDevice.BusId);
            return View(ioTDevice);
        }

        // GET: IoTDevices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ioTDevice = await _context.IoTDevices.FindAsync(id);
            if (ioTDevice == null)
            {
                return NotFound();
            }
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", ioTDevice.BusId);
            return View(ioTDevice);
        }

        // POST: IoTDevices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeviceId,DeviceName,DeviceIdentifier,Latitude,Longitude,Speed,BusId,LastUpdated")] IoTDevice ioTDevice)
        {
            if (id != ioTDevice.DeviceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ioTDevice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IoTDeviceExists(ioTDevice.DeviceId))
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
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", ioTDevice.BusId);
            return View(ioTDevice);
        }

        // GET: IoTDevices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ioTDevice = await _context.IoTDevices
                .Include(i => i.Bus)
                .FirstOrDefaultAsync(m => m.DeviceId == id);
            if (ioTDevice == null)
            {
                return NotFound();
            }

            return View(ioTDevice);
        }

        // POST: IoTDevices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ioTDevice = await _context.IoTDevices.FindAsync(id);
            if (ioTDevice != null)
            {
                _context.IoTDevices.Remove(ioTDevice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IoTDeviceExists(int id)
        {
            return _context.IoTDevices.Any(e => e.DeviceId == id);
        }
    }
}

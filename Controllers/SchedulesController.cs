using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YatriSewa.Models;

namespace YatriSewa.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly ApplicationContext _context;

        public SchedulesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Schedules
        public async Task<IActionResult> Index()
        {
            // Get the current user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Return unauthorized if the user ID can't be parsed
            }

            // Retrieve the logged-in user with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized(); // Return unauthorized if the user doesn't have an associated company
            }

            var companyId = user.BusCompany.CompanyId; // Get the company ID of the logged-in user

            // Retrieve only schedules belonging to the user's company
            var applicationContext = _context.Schedule_Table
                .Include(s => s.Bus)
                .Include(s => s.BusCompany)
                .Include(s => s.Driver)
                .Include(s => s.Route)
                .Where(s => s.BusCompanyId == companyId); // Filter by company

            return View(await applicationContext.ToListAsync());
        }


        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule_Table
                .Include(s => s.Bus)
                .Include(s => s.BusCompany)
                .Include(s => s.Driver)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            // Get the current user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // Retrieve the user's associated company
            var user = _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized();
            }

            var companyId = user.BusCompany.CompanyId;

            // Filter data by company
            ViewData["BusId"] = new SelectList(
                _context.Bus_Table.Where(b => b.CompanyId == companyId),
                "BusId",
                "BusName"
            );

            ViewData["BusCompanyId"] = new SelectList(
                _context.Company_Table.Where(c => c.CompanyId == companyId),
                "CompanyId",
                "CompanyName"
            );

            ViewData["DriverId"] = new SelectList(
                _context.Driver_Table.Where(d => d.CompanyId == companyId),
                "DriverId",
                "DriverName"
            );

            ViewData["RouteId"] = new SelectList(
                _context.Route_Table
                    .Where(r => r.CompanyID == companyId)
                    .Select(r => new {
                        RouteID = r.RouteID,
                        RouteDescription = r.StartLocation + " - " + r.Stops + " - " + r.EndLocation
                    }),
                "RouteID",
                "RouteDescription"
            );
            ViewData["BusCompanyId"] = companyId;
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetBusDetails(int busId)
        {
            var bus = await _context.Bus_Table
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(b => b.BusId == busId);

            if (bus == null)
            {
                return NotFound();
            }

            return Json(new
            {
                routeId = bus.RouteId,
                routeDescription = bus.Route != null
                    ? bus.Route.StartLocation + " - " + bus.Route.Stops + " - " + bus.Route.EndLocation
                    : null,
                driverId = bus.BusDriver?.DriverId,
                driverName = bus.BusDriver?.DriverName,
                price = bus.Price,
                availableSeats = bus.SeatCapacity
            });
          
        }




        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleId,BusId,RouteId,DepartureTime,ArrivalTime,Price,AvailableSeats,Status,DriverId,BusCompanyId")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", schedule.BusId);
            ViewData["BusCompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", schedule.BusCompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", schedule.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", schedule.RouteId);
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the schedule record with the provided ID
            var schedule = await _context.Schedule_Table.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            // Fetch the related bus information, including company and driver
            var bus = await _context.Bus_Table
                .Include(b => b.BusDriver)
                .Include(b => b.BusCompany)
                .FirstOrDefaultAsync(b => b.BusId == schedule.BusId);

            if (bus == null)
            {
                return NotFound();
            }

            // Fetch route information for the dropdown
            var routes = _context.Route_Table
                .Where(r => r.CompanyID == bus.CompanyId)
                .Select(r => new
                {
                    RouteID = r.RouteID,
                    RouteDescription = r.StartLocation + " - " + r.Stops + " - " + r.EndLocation
                })
                .ToList();

            // Fetch drivers associated with the company
            var drivers = _context.Driver_Table
                .Where(d => d.CompanyId == bus.CompanyId)
                .Select(d => new
                {
                    DriverId = d.DriverId,
                    DriverName = d.DriverName
                })
                .ToList();

            // Populate ViewData for dropdowns
            ViewData["BusId"] = new SelectList(
                _context.Bus_Table.Where(b => b.CompanyId == bus.CompanyId),
                "BusId",
                "BusName",
                schedule.BusId
            );

            ViewData["RouteId"] = new SelectList(
                routes,
                "RouteID",
                "RouteDescription",
                schedule.RouteId
            );

            ViewData["DriverId"] = new SelectList(
                drivers,
                "DriverId",
                "DriverName",
                schedule.DriverId
            );

            // Automatically set the BusCompanyId (no dropdown needed)
            ViewData["BusCompanyId"] = bus.CompanyId;
            ViewData["BusName"] = bus.BusName;
            return View(schedule);
        }


        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,BusId,RouteId,DepartureTime,ArrivalTime,Price,AvailableSeats,Status,DriverId,BusCompanyId")] Schedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.ScheduleId))
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
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", schedule.BusId);
            ViewData["BusCompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", schedule.BusCompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", schedule.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", schedule.RouteId);
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule_Table
                .Include(s => s.Bus)
                .Include(s => s.BusCompany)
                .Include(s => s.Driver)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedule_Table.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedule_Table.Remove(schedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedule_Table.Any(e => e.ScheduleId == id);
        }
    }
}

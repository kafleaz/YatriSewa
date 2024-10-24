using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;

namespace YatriSewa.Controllers
{
    public class OperatorController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<OperatorController> _logger;

        // Modify the constructor to accept ILogger<OperatorController>
        public OperatorController(ApplicationContext context, ILogger<OperatorController> logger)
        {
            _context = context;
            _logger = logger;  // Assign logger to the private field
        }

        [Authorize(Roles = "Operator")]
        public IActionResult OperatorDashboard()
        {
            return View();
        }
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> ListBus()
        {
            // Fetch buses with their related entities (BusCompany, BusDriver, Route)
            var buses = await _context.Bus_Table
                .Include(b => b.BusCompany) // Include the bus company
                .Include(b => b.BusDriver)  // Include the bus driver
                .Include(b => b.Route)      // Include the route
                .ToListAsync();

            // Fetch the company name from the first bus, assuming all buses belong to the same company
            var companyName = buses.FirstOrDefault()?.BusCompany?.CompanyName ?? "Unknown Company";

            // Pass the company name to the view
            ViewBag.CompanyName = companyName;

            // Return the list of buses to the view
            return View(buses);
        }


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

        //// GET: Buses/Create
        [Authorize(Roles = "Admin, Operator")]
        public IActionResult AddBus()
        {
            var routes = _context.Route_Table.Select(r => new {
                r.RouteID,
                RouteDescription = r.StartLocation + " - " + r.Stops + " - " + r.EndLocation
            }).ToList();
            ViewData["RouteId"] = new SelectList(routes, "RouteID", "RouteDescription");
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName");
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "DriverName");
            //ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation");
            return View();
        }



        //[Authorize(Roles = "Operator")]
        //public async Task<IActionResult> AddBus()
        //{
        //    // Get the current user's company (assuming User has a link to CompanyId)
        //    var currentUser = await _context.Users
        //        .Include(u => u.Company) // Assuming there is a navigation property for the user's company
        //        .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    if (currentUser?.Company == null)
        //    {
        //        return NotFound("Operator's company not found.");
        //    }

        //    // Automatically assign buses to the operator's company, no need for a dropdown for CompanyId
        //    var bus = new Bus
        //    {
        //        CompanyId = currentUser.Company.CompanyId // Automatically link the bus to the operator's company
        //    };

        //    ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation");
        //    ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "DriverName");
        //    return View(bus);
        //}

        [HttpPost]
        [Authorize(Roles = "Operator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBus([Bind("BusId,BusName,BusNumber,Description,SeatCapacity,Price,RouteId,DriverId")] Bus bus)
        {
            // Get the current user's UserId from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Retrieve UserId from claims

            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Get the current user including the BusCompany based on the UserId
            var currentUser = await _context.User_Table
                .Include(u => u.BusCompany)  // Load the associated BusCompany
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);  // Find user by UserId

            if (currentUser?.BusCompany == null)
            {
                return NotFound("Operator's company not found.");
            }

            // Automatically set the bus's company to the operator's company
            bus.CompanyId = currentUser.CompanyID ?? 0;  // Assign the current user's company ID

            // Check if a bus with the same BusNumber already exists in any company
            var existingBus = await _context.Bus_Table
                .Include(b => b.BusCompany)  // Include BusCompany to get the company name
                .FirstOrDefaultAsync(b => b.BusNumber == bus.BusNumber);

            if (existingBus != null)
            {
                var companyName = existingBus.BusCompany?.CompanyName ?? "Unknown";
                ModelState.AddModelError("BusNumber", $"The bus is already being managed by {companyName}.");
            }

            if (ModelState.IsValid)
            {
                // Optional assignment of RouteId and DriverId
                if (bus.RouteId == 0)
                {
                    bus.RouteId = null; // Allow assigning the route later
                }
                if (bus.DriverId == 0)
                {
                    bus.DriverId = null; // Allow assigning the driver later
                }

                try
                {
                    _context.Add(bus);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ListBus));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error occurred while saving the bus.");
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }

            // Repopulate dropdowns if form validation fails
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", bus.RouteId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "DriverName");

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


        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> SortList(string sortOrder)
        {
            // Store the current sort order in ViewBag for the view to know the current sorting state
            ViewBag.CurrentSort = sortOrder;

            // Set the toggling sort order for each column
            ViewBag.BusNameSortParm = String.IsNullOrEmpty(sortOrder) || sortOrder == "busname_asc" ? "busname_desc" : "busname_asc";
            ViewBag.BusNumberSortParm = sortOrder == "busnumber_asc" ? "busnumber_desc" : "busnumber_asc";
            ViewBag.SeatCapacitySortParm = sortOrder == "seatcapacity_asc" ? "seatcapacity_desc" : "seatcapacity_asc";

            // Fetch the data from the database
            var buses = from b in _context.Bus_Table.Include(b => b.BusCompany)
                        select b;

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "busname_desc":
                    buses = buses.OrderByDescending(b => b.BusName);
                    break;
                case "busnumber_asc":
                    buses = buses.OrderBy(b => b.BusNumber);
                    break;
                case "busnumber_desc":
                    buses = buses.OrderByDescending(b => b.BusNumber);
                    break;
                case "seatcapacity_asc":
                    buses = buses.OrderBy(b => b.SeatCapacity);
                    break;
                case "seatcapacity_desc":
                    buses = buses.OrderByDescending(b => b.SeatCapacity);
                    break;
                default:
                    buses = buses.OrderBy(b => b.BusName); // Default sorting by BusName ascending
                    break;
            }

            // Return the view with sorted bus data
            return View(await buses.ToListAsync());
        }



    }
}

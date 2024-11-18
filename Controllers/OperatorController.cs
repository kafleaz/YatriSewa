using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;
using Route = YatriSewa.Models.Route;

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
            // Get the current user ID as a string
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to int
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Retrieve the logged-in user from the User_Table with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany) // Assuming User_Table has a relation with BusCompany
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var companyId = user.BusCompany.CompanyId;

            // Fetch only buses associated with the user's company
            var buses = await _context.Bus_Table
                .Include(b => b.BusCompany)
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .Where(b => b.CompanyId == companyId) // Filter by company ID
                .ToListAsync();

            // Set the company name based on the user's company
            ViewBag.CompanyName = user.BusCompany.CompanyName;

            return View(buses);
        }




        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> BusDetails(int? id)
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

        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> AddBus()
        {
            // Get the current user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // Retrieve the user's associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized();
            }

            var companyId = user.BusCompany.CompanyId;

            // Fetch routes associated with the user's company
            var routes = _context.Route_Table
                .Where(r => r.CompanyID == companyId)
                .Select(r => new {
                    r.RouteID,
                    RouteDescription = r.StartLocation + " - " + r.Stops + " - " + r.EndLocation
                })
                .ToList();

            // Fetch drivers indirectly associated with the user's company through Bus
            // Fetch drivers assigned to the user's company
            var drivers = _context.Driver_Table
                .Where(d => d.CompanyId == companyId) // Check for assigned drivers
                .Select(d => new {
                    d.DriverId,
                    d.DriverName
                })
                .ToList();

            

            // Populate ViewData with filtered routes and drivers
            ViewData["RouteId"] = new SelectList(routes, "RouteID", "RouteDescription");
            ViewData["DriverId"] = new SelectList(drivers, "DriverId", "DriverName");

            // Pre-fill the CompanyId for the logged-in user’s company
            ViewData["CompanyId"] = companyId;

            return View();
        }



        [HttpPost]
        [Authorize(Roles = "Admin, Operator")]
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
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> EditBus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // Retrieve the logged-in user with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized();
            }

            var companyId = user.BusCompany.CompanyId;

            // Fetch the bus to edit, ensuring it belongs to the user's company
            var bus = await _context.Bus_Table
                .FirstOrDefaultAsync(b => b.BusId == id && b.CompanyId == companyId);

            if (bus == null)
            {
                return NotFound(); // Bus not found or doesn't belong to the user's company
            }

            // Fetch routes associated with the user's company
            var routes = _context.Route_Table
                .Where(r => r.CompanyID == companyId)
                .Select(r => new {
                    r.RouteID,
                    RouteDescription = r.StartLocation + " - " + r.Stops + " - " + r.EndLocation
                })
                .ToList();

            // Fetch drivers associated with the user's company (indirectly through Bus)
            // Fetch drivers directly assigned to the user's company
            var drivers = _context.Driver_Table
                .Where(d => d.CompanyId == companyId) // Directly assigned drivers
                .Select(d => new {
                    d.DriverId,
                    d.DriverName
                })
                .ToList();


            // Populate ViewData with filtered routes, drivers, and company information
            ViewData["RouteId"] = new SelectList(routes, "RouteID", "RouteDescription", bus.RouteId);
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", bus.CompanyId);
            ViewData["DriverId"] = new SelectList(drivers, "DriverId", "DriverName", bus.DriverId);

            return View(bus);
        }


        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBus(int id, [Bind("BusId,BusName,BusNumber,Description,SeatCapacity,Price,CompanyId,RouteId,DriverId")] Bus bus)
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
                return RedirectToAction(nameof(ListBus));
            }
            ViewData["CompanyId"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", bus.CompanyId);
            ViewData["DriverId"] = new SelectList(_context.Driver_Table, "DriverId", "Address", bus.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Route_Table, "RouteID", "EndLocation", bus.RouteId);
            return View(bus);
        }

        // GET: Buses/Delete/5
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> DeleteBus(int? id)
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
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost, ActionName("DeleteBus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bus = await _context.Bus_Table.FindAsync(id);
            if (bus != null)
            {
                _context.Bus_Table.Remove(bus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListBus));
        }

        private bool BusExists(int id)
        {
            return _context.Bus_Table.Any(e => e.BusId == id);
        }

        //==============================Route Control================================================
       
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> ListRoute()
        {
            // Get the current user ID as a string
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to int
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Retrieve the logged-in user from the User_Table with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany) // Assuming User_Table has a relation with BusCompany
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var companyId = user.BusCompany.CompanyId;

            // Fetch only routes associated with the user's company
            var routes = await _context.Route_Table
                .Include(r => r.BusCompany)
                .Where(r => r.CompanyID == companyId) // Filter by company ID
                .ToListAsync();

            // Set the company name based on the user's company
            ViewBag.CompanyName = user.BusCompany.CompanyName;

            // Return the list of routes to the view
            return View(routes);
        }



        // GET: Routes/Details/5
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> RouteDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route_Table
                .Include(r => r.BusCompany)
                .FirstOrDefaultAsync(m => m.RouteID == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // GET: Routes/Create
        [Authorize(Roles = "Operator, Admin")]
        public IActionResult AddRoute()
        {
            ViewData["CompanyID"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName");
            return View();
        }

        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoute([Bind("RouteID,StartLocation,Stops,EndLocation,EstimatedTime")] Route route)  // Removed CompanyID from the Bind
        {
            // Retrieve UserId from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the current user, including the BusCompany based on the UserId
            var currentUser = await _context.User_Table
                .Include(u => u.BusCompany)  // Load the associated BusCompany
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);  // Find user by UserId

            if (currentUser?.BusCompany == null)
            {
                return NotFound("Operator's company not found.");
            }

            // Automatically set the route's CompanyID to the operator's company
            route.CompanyID = currentUser.BusCompany.CompanyId;  // Assign the current user's company ID

            if (ModelState.IsValid)
            {
                // Save the new route with the operator's company ID
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListRoute));
            }

            // If there's an error, reload the view
            return View(route);
        }



        // GET: Routes/Edit/5
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> EditRoute(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route_Table.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }
            ViewData["CompanyID"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", route.CompanyID);
            return View(route);
        }

        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoute(int id, [Bind("RouteID,StartLocation,Stops,EndLocation,EstimatedTime,CompanyID")] Route route)
        {
            if (id != route.RouteID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(route);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.RouteID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListRoute));
            }
            ViewData["CompanyID"] = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", route.CompanyID);
            return View(route);
        }

        // GET: Routes/Delete/5
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> DeleteRoute(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route_Table
                .Include(r => r.BusCompany)
                .FirstOrDefaultAsync(m => m.RouteID == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // POST: Routes/Delete/5
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost, ActionName("DeleteRoute")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Route_Table.FindAsync(id);
            if (route != null)
            {
                _context.Route_Table.Remove(route);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListRoute));
        }

        private bool RouteExists(int id)
        {
            return _context.Route_Table.Any(e => e.RouteID == id);
        }


//===================================Service Control==============================

        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> ListService()
        {
            // Get the current user ID as a string
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to int
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Retrieve the logged-in user from the User_Table with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany) // Assuming User_Table has a relation with BusCompany
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized(); // Handle unauthorized access
            }

            var companyId = user.BusCompany.CompanyId;

            // Fetch only services associated with the user's company
            var services = await _context.Service_Table
                .Include(s => s.Bus)
                .Where(s => s.Bus != null && s.Bus.CompanyId == companyId) // Filter services by the bus's company ID
                .ToListAsync();

            return View(services);
        }

        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> ServiceDetails(int? id)
        { 
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Service_Table
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> AddService()
        {
            // Get the current user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // Retrieve the logged-in user with their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany) // Ensure we load the BusCompany
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized();
            }

            var companyId = user.BusCompany.CompanyId;

            // Filter buses by the user's company
            var buses = _context.Bus_Table
                .Where(b => b.CompanyId == companyId)
                .Select(b => new { b.BusId, b.BusName })
                .ToList();

            // Populate ViewData with the filtered buses and BusType options
            ViewData["BusType"] = new SelectList(Enum.GetValues(typeof(BusType)));
            ViewData["BusId"] = new SelectList(buses, "BusId", "BusName");

            return View();
        }

        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService([Bind("ServiceId,Wifi,AC,BusType,SafetyFeatures,Essentials,Snacks,BusId")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListService));
            }
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", service.BusId);
            return View(service);
        }


        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> EditService(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Include the related Bus entity
            var service = await _context.Service_Table
                .Include(s => s.Bus) // Load the associated Bus
                .FirstOrDefaultAsync(m => m.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }
            if (service.Bus == null)
            {
                return View(service);
            }

            // Pass the BusName to ViewData so it can be displayed in the view
            ViewData["BusName"] = service.Bus.BusName; // You can also use ViewBag if you prefer
            ViewData["BusType"] = new SelectList(Enum.GetValues(typeof(BusType)));

            return View(service);
        }

        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, [Bind("ServiceId,Wifi,AC,BusType,SafetyFeatures,Essentials,Snacks,BusId")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListService));
            }
            ViewData["BusId"] = new SelectList(_context.Bus_Table, "BusId", "BusName", service.BusId);
            return View(service);
        }

        // GET: Services/Delete/5
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> DeleteService(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Service_Table
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost, ActionName("DeleteService")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Service_Table.FindAsync(id);
            if (service != null)
            {
                _context.Service_Table.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListService));
        }

        private bool ServiceExists(int id)
        {
            return _context.Service_Table.Any(e => e.ServiceId == id);
        }

        //============================Bus Drivers==============================
        [Authorize(Roles = "Operator, Admin, Driver")]
        public async Task<IActionResult> ListDrivers()
        {
            // Get the current user ID from claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Convert the user ID to an integer
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Retrieve the logged-in user, including their associated company
            var user = await _context.User_Table
                .Include(u => u.BusCompany) // Use the correct navigation property for Company_Table
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusCompany == null)
            {
                return Unauthorized(); // Handle unauthorized access or no associated company
            }

            var companyId = user.BusCompany.CompanyId; // Use the navigation property to get CompanyID

            // Fetch only drivers associated with the user's company
            var drivers = await _context.Driver_Table
                .Include(d => d.User) // Include user details if needed
                .Where(d => d.CompanyId == companyId) // Filter by company ID
                .ToListAsync();

            // Optionally, set the company name or other details in ViewBag if needed
            ViewBag.CompanyName = user.BusCompany.CompanyName;

            // Return the list of drivers to the view
            return View(drivers);
        }

        public async Task<IActionResult> SearchDriver(string licenseNumber)
        {
            if (string.IsNullOrEmpty(licenseNumber))
            {
                // Handle case where no input is provided
                ModelState.AddModelError("", "Please enter a license number.");
                return View("SearchDriver");
            }

            // Retrieve the driver based on the license number
            var driver = await _context.Driver_Table
                .Include(d => d.User) // Include related User details if needed
                .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber);

            if (driver == null)
            {
                // Handle case where the driver is not found
                ViewBag.ErrorMessage = "No driver found with the provided license number.";
                return View("SearchDriver");
            }

            // Pass the found driver to the view
            return View("DriverDetails", driver);
        }

        // GET: BusDrivers/Details/5
        public async Task<IActionResult> DriverDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busDriver = await _context.Driver_Table
                //.Include(b => b.User)
                .Include(b => b.BusCompany) // Include the related BusCompany
                .FirstOrDefaultAsync(m => m.DriverId == id);

            if (busDriver == null)
            {
                return NotFound();
            }

            return View(busDriver);
        }



        // GET: BusDrivers/Create
        //public IActionResult AddDriver()
        //{
        //    ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name");
        //    return View();
        //}

        // POST: BusDrivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize(Roles = "Operator, Admin, Driver")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddDriver([Bind("DriverId,DriverName,LicenseNumber,PhoneNumber,Address,DateOfBirth,LicensePhotoPath,IsAvailable,UserId")] BusDriver busDriver)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(busDriver);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(ListDrivers));
        //    }
        //    ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", busDriver.UserId);
        //    return View(busDriver);
        //}
        [Authorize(Roles = "Operator, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDriver(int driverId)
        {
            // Get the current user ID from claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Validate and parse the user ID
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(); // Handle unauthorized access
            }

            // Retrieve the current logged-in user with their company
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CompanyID == null)
            {
                return Unauthorized(); // Handle unauthorized access or missing company
            }

            // Find the driver by ID
            var driver = await _context.Driver_Table.FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver == null)
            {
                return NotFound("Driver not found.");
            }

            if (!driver.IsAvailable)
            {
                return BadRequest("Driver is already assigned to a company.");
            }

            // Update the driver details
            driver.CompanyId = user.CompanyID.Value;
            driver.IsAvailable = false;

            // Save changes to the database
            _context.Update(driver);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListDrivers));
        }



        // GET: BusDrivers/Edit/5
        public async Task<IActionResult> EditDriver(int? id)
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
        public async Task<IActionResult> EditDriver(int id, [Bind("DriverId,DriverName,LicenseNumber,PhoneNumber,Address,DateOfBirth,LicensePhotoPath,IsAvailable,UserId")] BusDriver busDriver)
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
                return RedirectToAction(nameof(ListDrivers));
            }
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", busDriver.UserId);
            return View(busDriver);
        }

        // GET: BusDrivers/Delete/5
        public async Task<IActionResult> DeleteDriver(int? id)
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
        [HttpPost, ActionName("DeleteDriver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            // Find the driver in the database
            var busDriver = await _context.Driver_Table.FindAsync(id);
            if (busDriver != null)
            {
                // Update the properties instead of deleting the record
                busDriver.IsAvailable = true;
                busDriver.CompanyId = null;

                // Mark the record as modified
                _context.Driver_Table.Update(busDriver);

                // Save the changes to the database
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ListDrivers));
        }

        //public async Task<IActionResult> DeleteDriver(int id)
        //{
        //    var busDriver = await _context.Driver_Table.FindAsync(id);
        //    if (busDriver != null)
        //    {
        //        _context.Driver_Table.Remove(busDriver);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(ListDrivers));
        //}

        private bool BusDriverExists(int id)
        {
            return _context.Driver_Table.Any(e => e.DriverId == id);
        }

    }


}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;
using YatriSewa.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Controllers
{
    public class OperatorController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<OperatorController> _logger;
        private readonly IDriverService _driverService;
        private readonly IOperatorService _operatorService;

        // Modify the constructor to accept ILogger<OperatorController>

        public OperatorController(ApplicationContext context, IOperatorService operatorService,IDriverService driverService, ILogger<OperatorController> logger)
        {
            _context = context; // Initialize context
            _operatorService = operatorService; // Initialize the service
            _logger = logger;
            _driverService = driverService;
            // Assign logger to the private field
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
            var buses = await _operatorService.GetBusesByUserIdAsync(userId.ToString()); // Adjusted based on user ID type
            return View(buses); // Pass buses to view
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

        //adding bus section
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

        //[Authorize(Roles = "Operator, Admin")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]

        //public async Task<IActionResult> AddRoute([Bind("RouteID,StartLocation,Stops,EndLocation,EstimatedTime")] Route route)  // Removed CompanyID from the Bind
        //{
        //    // Retrieve UserId from claims
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Get the current user, including the BusCompany based on the UserId
        //    var currentUser = await _context.User_Table
        //        .Include(u => u.BusCompany)  // Load the associated BusCompany
        //        .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);  // Find user by UserId

        //    if (currentUser?.BusCompany == null)
        //    {
        //        return NotFound("Operator's company not found.");
        //    }

        //    // Automatically set the route's CompanyID to the operator's company
        //    route.CompanyID = currentUser.BusCompany.CompanyId;  // Assign the current user's company ID

        //    if (ModelState.IsValid)
        //    {
        //        // Save the new route with the operator's company ID
        //        _context.Add(route);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(ListRoute));
        //    }

        //    // If there's an error, reload the view
        //    return View(route);
        //}

        [HttpPost]
        [Authorize(Roles = "Operator, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoute([Bind("RouteID,StartLocation,Stops,EndLocation,EstimatedTime,EndLongitude,EndLatitude,StartLongitude,StartLatitude")] Route route)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentUser = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (currentUser?.BusCompany == null)
            {
                return NotFound("Operator's company not found.");
            }

            route.CompanyID = currentUser.BusCompany.CompanyId;

            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListRoute));
            }

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

        private bool BusDriverExists(int id)
        {
            return _context.Driver_Table.Any(e => e.DriverId == id);
        }
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var operatorUser = await _context.User_Table
                .Include(u => u.BusCompany) // Include company info
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId && u.Role == UserRole.Operator);

            if (operatorUser == null)
            {
                return NotFound("Operator profile not found.");
            }

            return View(operatorUser);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var operatorUser = await _context.User_Table.FindAsync(userId);
            if (operatorUser == null)
            {
                return NotFound();
            }

            // Update fields
            operatorUser.Name = model.Name;
            operatorUser.PhoneNo = model.PhoneNo;

            _context.Update(operatorUser);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var operatorUser = await _context.User_Table.FindAsync(userId);
            if (operatorUser == null)
            {
                return NotFound();
            }

            // Verify the current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, operatorUser.Password))
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View("Profile", operatorUser);
            }

            // Check if the new passwords match
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New passwords do not match.");
                return View("Profile", operatorUser);
            }

            // Hash and update the password
            operatorUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Update(operatorUser);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";
            return RedirectToAction(nameof(Profile));
        }



      
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> PassengerList(int scheduleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var passengers = await _operatorService.GetPassengersByScheduleIdAsync(scheduleId);

            if (!passengers.Any())
            {
                ViewBag.Message = "No passengers have purchased tickets for this schedule.";
                return View(new List<dynamic>()); // ✅ Pass an empty List<dynamic> if no passengers
            }

            return View(passengers.ToList()); // ✅ Convert to List<dynamic>
        }


        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> ScheduledBuses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var operatorUser = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (operatorUser?.BusCompany == null)
            {
                return NotFound("No associated company found for this operator.");
            }

            var companyId = operatorUser.BusCompany.CompanyId;
            var today = DateTime.UtcNow.Date;

            var scheduledBuses = await _context.Schedule_Table
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Where(s => s.Bus.CompanyId == companyId && s.DepartureTime.Date == today)
                .ToListAsync();

            if (!scheduledBuses.Any())
            {
                ViewBag.Message = "No scheduled buses available for today.";
                return View(new List<Schedule>());
            }

            return View(scheduledBuses);
        }

    }


}

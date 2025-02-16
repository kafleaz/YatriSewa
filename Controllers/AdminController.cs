using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;
using YatriSewa.Services;
using YatriSewa.Services.Interfaces;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IOperatorService _operatorService;
        private readonly IDriverService _driverService;

        // Modify the constructor to accept ILogger<OperatorController>
        public AdminController(ApplicationContext context, IOperatorService operatorService, IDriverService driverService, ILogger<AdminController> logger)
        {
            _context = context; // Assigning the database context
            _operatorService = operatorService; // Assigning the operator service
            _driverService = driverService;
            _logger = logger; // Assigning the logger

        }


        [HttpGet("api/getstats")]
        public IActionResult GetStats()
        {
            // Query database for counts
            var usersCount = _context.User_Table.Count();
            var busesCount = _context.Bus_Table.Count();
            var operatorsCount = _context.User_Table.Count(user => user.Role == UserRole.Operator);


            // Return stats as JSON
            return Ok(new
            {
                Users = usersCount,
                Buses = busesCount,
                Operators = operatorsCount
            });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Operator, Admin")]



        // GET: Admin/Operators
        public async Task<IActionResult> ListOperators()
        {
            try
            {
                var operators = await _operatorService.GetAllOperatorAsync();
                return View(operators);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching operators: {ex.Message}");
                return View("Error", new { message = "Unable to fetch operators. Please try again later." });
            }
        }

        [Route("Admin/ListBus")]
        public async Task<IActionResult> ListBus(int? companyId = null)
        {
            try
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (User.IsInRole("Admin") && !companyId.HasValue)
                {
                    var allCompanies = await _operatorService.GetAllOperatorAsync();
                    var allBuses = new List<Bus>();

                    foreach (var company in allCompanies)
                    {
                        var companyBuses = await _operatorService.GetBusesByCompanyIdAsync(company.CompanyId);

                        // Add RouteDescription for each bus's route
                        foreach (var bus in companyBuses)
                        {
                            if (bus.Route != null)
                            {
                                ViewBag.Description= $"{bus.Route.StartLocation} - {bus.Route.Stops} - {bus.Route.EndLocation}";
                            }
                        }

                        allBuses.AddRange(companyBuses);
                    }

                    if (!allBuses.Any())
                    {
                        ViewBag.Message = "No buses are registered in the system.";
                        return View(new List<Bus>());
                    }

                    ViewBag.CompanyName = "All Companies";
                    return View(allBuses);
                }

                // Fetch the current user and their associated company
                var user = await _operatorService.GetCurrentUserWithCompanyAsync(userId);

                var companyToFetch = companyId ?? user?.BusCompany?.CompanyId;
                var buses = await _operatorService.GetBusesByCompanyIdAsync(companyToFetch.Value);

                // Add RouteDescription for each bus's route
                foreach (var bus in buses)
                {
                    if (bus.Route != null)
                    {
                        ViewBag.Description = $"{bus.Route.StartLocation} - {bus.Route.Stops} - {bus.Route.EndLocation}";
                    }
                }

                return View(buses);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching buses: {ex.Message}");
                return View("Error", new { message = "Unable to fetch buses. Please try again later." });
            }
        }





        public async Task<IActionResult> BusDetails(int id)
        {
            try
            {
                var bus = await _operatorService.GetBusDetailsAsync(id);
                if (bus == null)
                {
                    return NotFound("Bus not found.");
                }
                return View(bus);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching bus details for ID {id}: {ex.Message}");
                return View("Error", new { message = "Unable to fetch bus details. Please try again later." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Operator")]
        [Route("Admin/AddBus")]
        public async Task<IActionResult> AddBus()
        {
            try
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _operatorService.GetCurrentUserWithCompanyAsync(userId);

                if (user == null || user.BusCompany == null)
                {
                    ViewData["RouteID"] = new SelectList(Enumerable.Empty<SelectListItem>(), "RouteID", "RouteDescription");
                    ViewData["DriverID"] = new SelectList(Enumerable.Empty<SelectListItem>(), "DriverID", "DriverName");
                    ModelState.AddModelError("", "You are not associated with any company.");
                    return View();
                }

                var companyId = user.BusCompany.CompanyId;

                // Fetch routes and drivers associated with the user's company
                var routes = await _operatorService.GetRoutesByCompanyIdAsync(companyId);
                var drivers = await _operatorService.GetDriversByCompanyIdAsync(companyId);

                ViewData["RouteID"] = routes != null && routes.Any()
                    ? new SelectList(routes, "RouteID", "RouteDescription")
                    : new SelectList(Enumerable.Empty<SelectListItem>(), "RouteID", "RouteDescription");

                ViewData["DriverID"] = drivers != null && drivers.Any()
                    ? new SelectList(drivers, "DriverID", "DriverName")
                    : new SelectList(Enumerable.Empty<SelectListItem>(), "DriverID", "DriverName");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading AddBus view: {ex.Message}");
                return View("Error", new { message = "Unable to load Add Bus view. Please try again later." });
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Operator")]
        [Route("Admin/AddBus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBus([Bind("BusName, BusNumber, Description, SeatCapacity, Price, RouteId, DriverId")] Bus bus)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate ViewData to avoid empty dropdowns
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _operatorService.GetCurrentUserWithCompanyAsync(userId);

                if (user?.BusCompany != null)
                {
                    var routes = await _operatorService.GetRoutesByCompanyIdAsync(user.BusCompany.CompanyId);
                    var drivers = await _operatorService.GetDriversByCompanyIdAsync(user.BusCompany.CompanyId);

                    ViewData["RouteID"] = new SelectList(routes, "RouteID", "RouteDescription", bus.RouteId);
                    ViewData["DriverID"] = new SelectList(drivers, "DriverID", "DriverName", bus.DriverId);
                }

                return View(bus);
            }

            try
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                await _operatorService.AddBusAsync(bus, userId);

                TempData["SuccessMessage"] = "Bus added successfully!";
                return RedirectToAction(nameof(ListBus));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding bus: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding the bus. Please try again.");
                return View(bus);
            }
        }

        public async Task<IActionResult> ListRoutes(string userId)
        {
            try
            {
                var routes = await _context.Route_Table.ToListAsync();
                return View(routes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching routes: {ex.Message}");
                return View("Error", new { message = "Unable to fetch routes. Please try again later." });
            }
        }

        public async Task<IActionResult> RouteDetails(int id)
        {
            try
            {
                var route = await _operatorService.GetRouteDetailsAsync(id);
                if (route == null)
                {
                    return NotFound("Route not found.");
                }
                return View(route);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching route details for ID {id}: {ex.Message}");
                return View("Error", new { message = "Unable to fetch route details. Please try again later." });
            }
        }

        //DriverList
        public async Task<IActionResult>ListDrivers()
        {
            var drivers = await _driverService.GetAllDriversAsync(); // Fetch all drivers
            return View(drivers); // Pass data to the view
        }

        [Route("Admin/JourneyList")]
        public async Task<IActionResult> TodayJourneys()
        {
            var journeys = await _driverService.GetTodayJourneysAsync();

            if (!journeys.Any())
            {
                ViewBag.Message = "No journeys scheduled for today.";
                return View(new List<object>());
            }

            return View(journeys);
        }


        //fetching request forms and approving it
        [HttpGet("api/getrequests")]
        public async Task<IActionResult> GetRequests()
       
            {
                var operatorRequests = await _context.Company_Table
                    .Where(r => !_context.User_Table.Any(u => u.CompanyID == r.CompanyId && u.Role == UserRole.Operator))
                    .Select(r => new
                    {
                        id = r.CompanyId,
                        name = r.CompanyName,
                        contactInfo = r.ContactInfo,
                        userId = r.CompanyId // Just store the CompanyId for lookup later
                    })
                    .ToListAsync();

                var driverRequests = await _context.Driver_Table
                    .Where(r => !_context.User_Table.Any(u => u.DriverId == r.DriverId && u.Role == UserRole.Driver))
                    .Select(r => new
                    {
                        id = r.DriverId,
                        name = r.DriverName,
                        contactInfo = r.PhoneNumber,
                        userId = r.DriverId // Just store the DriverId for lookup later
                    })
                    .ToListAsync();

                // Apply the null-check and fetch requestedBy in-memory (after fetching from DB)
                var operatorRequestsWithUser = operatorRequests
                    .Select(r => new
                    {
                        r.id,
                        r.name,
                        r.contactInfo,
                        requestedBy = _context.User_Table.FirstOrDefault(u => u.UserId == r.userId)?.Name ?? "Unknown"
                    })
                    .ToList();

                var driverRequestsWithUser = driverRequests
                    .Select(r => new
                    {
                        r.id,
                        r.name,
                        r.contactInfo,
                        requestedBy = _context.User_Table.FirstOrDefault(u => u.UserId == r.userId)?.Name ?? "Unknown"
                    })
                    .ToList();

                return Json(new
                {
                    operatorRequests = operatorRequestsWithUser,
                    driverRequests = driverRequestsWithUser
                });
            }


            // Fetch request details by ID
            [HttpGet("api/getrequestdetails/{id}")]
        public async Task<IActionResult> GetRequestDetails(int id)
        {
            // Check if the request is for an operator
            var operatorRequest = await _context.Company_Table.FirstOrDefaultAsync(r => r.CompanyId == id);
            if (operatorRequest != null)
            {
                return Json(new
                {
                    id = operatorRequest.CompanyId,
                    name = operatorRequest.CompanyName,
                    contactInfo = operatorRequest.ContactInfo
                });
            }

            // Check if the request is for a driver
            var driverRequest = await _context.Driver_Table.FirstOrDefaultAsync(r => r.DriverId == id);
            if (driverRequest != null)
            {
                return Json(new
                {
                    id = driverRequest.DriverId,
                    name = driverRequest.DriverName,
                    contactInfo = driverRequest.PhoneNumber
                });
            }

            return NotFound("Request not found.");
        }

        [HttpGet("Admin/ApproveRequest")]
        public async Task<IActionResult> ApproveRequest(int requestId, string role)
        {
            if (role == "Operator")
            {
                var company = await _context.Company_Table.FirstOrDefaultAsync(c => c.CompanyId == requestId);
                if (company == null) return NotFound("Operator request not found.");

                return View("OperatorDetails", company); // Show Operator details
            }
            else if (role == "Driver")
            {
                var driver = await _context.Driver_Table.FirstOrDefaultAsync(d => d.DriverId == requestId);
                if (driver == null) return NotFound("Driver request not found.");

                return View("DriverDetails", driver); // Show Driver details
            }

            return BadRequest("Invalid role.");
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(int userId, string role)
        {
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound("User not found.");

            user.Role = role == "Operator" ? UserRole.Operator : UserRole.Driver;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Role successfully assigned.";
            return RedirectToAction("AdminDashboard");
        }



    }










}




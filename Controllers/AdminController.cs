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
                // Get the user ID of the currently logged-in user
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Admin: Fetch all buses if no companyId is provided
                if (User.IsInRole("Admin") && !companyId.HasValue)
                {
                    var allCompanies = await _operatorService.GetAllOperatorAsync();
                    var allBuses = new List<Bus>();

                    foreach (var company in allCompanies)
                    {
                        var companyBuses = await _operatorService.GetBusesByCompanyIdAsync(company.CompanyId);
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

                if (user?.BusCompany == null && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("User does not belong to a company.");
                    ViewBag.Message = "You do not belong to a company. No buses to display.";
                    return View(new List<Bus>());
                }

                // Determine the company to fetch buses for
                var companyToFetch = companyId ?? user?.BusCompany?.CompanyId;

                // Prevent unauthorized access to another company's buses
                if (!User.IsInRole("Admin") && user?.BusCompany?.CompanyId != companyToFetch)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Fetch buses by CompanyId
                var buses = await _operatorService.GetBusesByCompanyIdAsync(companyToFetch.Value);

                if (buses == null || !buses.Any())
                {
                    ViewBag.Message = "No buses found for this company.";
                    return View(new List<Bus>());
                }

                if (companyId.HasValue)
                {
                    // Fetch the specific company's name based on the provided companyId
                    var company = await _operatorService.GetCompanyByIdAsync(companyId.Value);
                    ViewBag.CompanyName = company?.CompanyName ?? "Unknown Company";
                }
                else
                {
                    // Use the logged-in user's company name
                    ViewBag.CompanyName = user?.BusCompany?.CompanyName ?? "Unknown Company";
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

        [Authorize(Roles = "Admin")]
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
                    return RedirectToAction("Index", "Home");
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



        // POST: Admin/AddBus
        [HttpPost]
        [Route("Admin/AddBus")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> AddBus([Bind("BusName, BusNumber, Description, SeatCapacity, Price, RouteId, DriverId")] Bus bus)
        {
            if (!ModelState.IsValid)
            {
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


        //fetching passengerlist

    }
}



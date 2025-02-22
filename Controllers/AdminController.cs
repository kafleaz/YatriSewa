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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using BCrypt.Net;

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

        public async Task<IActionResult> ListRoute(string userId)
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

      
        public async Task<IActionResult> TodayJourneys()
        {
            var journeys = await _driverService.GetJourneyListAsync();

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
                        licenseNumber = r.LicenseNumber,
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
                      
                    })
                    .ToList();

                var driverRequestsWithUser = driverRequests
                    .Select(r => new
                    {
                        r.id,
                        r.name,
                        r.contactInfo,
                        r.licenseNumber,
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
                    contactInfo = driverRequest.PhoneNumber,
                    licenseNumber = driverRequest.LicenseNumber
                });
            }

            return NotFound("Request not found.");
        }
        [HttpGet]
        public async Task<IActionResult> OperatorDetails()
        {
            var operatorRequests = await _context.Company_Table
                .Include(c => c.User)
                .Where(c => c.User.Role == UserRole.Passenger)
                .ToListAsync();

            return View(operatorRequests);
        }

        // ✅ Driver Details Page (Renders UI)
        [HttpGet]
        public async Task<IActionResult> DriverDetails()
        {
            var driverRequests = await _context.Driver_Table
                .Include(d => d.User)
                .Where(d => d.User.Role == UserRole.Passenger)
                .ToListAsync();

            return View(driverRequests);
        }

        // ✅ API: Get Operator Details
        [HttpGet]
        [Route("Admin/GetOperatorDetails")]
        public async Task<IActionResult> GetOperatorDetails(int companyId)
        {
            var operatorDetails = await _context.Company_Table
                .Include(c => c.User)
                .Where(c => c.CompanyId == companyId)
                .Select(c => new
                {
                    id = c.CompanyId,
                    name = c.CompanyName,
                    owner = c.User.Name,
                    contactInfo = c.ContactInfo,
                    regNo = c.Reg_No,
                    vatPan = c.VAT_PAN,
                    address = c.CompanyAddress,
                    email = c.User.Email,
                    vatPanPhotoPath = c.VAT_PAN_PhotoPath
                })
                .FirstOrDefaultAsync();

            if (operatorDetails == null)
            {
                return NotFound("Operator not found.");
            }

            return Json(operatorDetails);
        }

        // ✅ API: Get Driver Details
        [HttpGet]
        [Route("Admin/GetDriverDetails")]
        public async Task<IActionResult> GetDriverDetails(int driverId)
        {
            var driverDetails = await _context.Driver_Table
                .Include(d => d.User)
                .Where(d => d.DriverId == driverId)
                .Select(d => new
                {
                    id = d.DriverId,
                    name = d.DriverName,
                    phoneNumber = d.PhoneNumber,
                    licenseNumber = d.LicenseNumber,
                    address = d.Address,
                    dateOfBirth = d.DateOfBirth.ToString("yyyy-MM-dd"),
                    licensePhotoPath = d.LicensePhotoPath
                })
                .FirstOrDefaultAsync();

            if (driverDetails == null)
            {
                return NotFound("Driver not found.");
            }

            return Json(driverDetails);
        }

        // ✅ Approve Operator Request
        [HttpPost]
        public async Task<IActionResult> ApproveOperator(int companyId)
        {
            var company = await _context.Company_Table.FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null)
            {
                return NotFound("Operator request not found.");
            }

            // ✅ Fetch the correct UserId from Company_Table
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == company.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // ✅ Step 1: Assign role first
            user.Role = UserRole.Operator;

            // ✅ Step 2: Assign CompanyID only if it's NULL
            if (user.CompanyID == null)
            {
                user.CompanyID = company.CompanyId;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("OperatorDetails");
        }


        // ✅ Approve Driver Request
        [HttpPost]
        public async Task<IActionResult> ApproveDriver(int driverId)
        {
            var driver = await _context.Driver_Table.FirstOrDefaultAsync(d => d.DriverId == driverId);
            if (driver == null)
            {
                return NotFound("Driver request not found.");
            }

            // ✅ Fetch the correct UserId from Driver_Table
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == driver.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // ✅ Step 1: Assign role first
            user.Role = UserRole.Driver;

            // ✅ Step 2: Assign DriverID only if it's NULL
            if (user.DriverId == null)
            {
                user.DriverId = driver.DriverId;
            }
            // ✅ Step 3: Set IsAvailable to true (since driver is approved)
            driver.IsAvailable = true;

            _context.Update(user);
            _context.Update(driver);
            _context.Entry(driver).Property(d => d.IsAvailable).IsModified = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("DriverDetails");
        }

       
        // Helper method to check if a bus exists
        private bool BusExists(int id)
        {
            return _context.Bus_Table.Any(e => e.BusId == id);
        }

        //fetching all users of system 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActiveUsers()
        {
            var users = await _context.User_Table
                .Include(u => u.BusCompany) // Include bus company information
                .Where(u => u.IsVerified == true) // Filter active users
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> EditUsers(int id)
        {
            
            var user = await _context.User_Table.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Role = new SelectList(Enum.GetValues(typeof(UserRole)));
            ViewBag.CompanyID = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", user.CompanyID);
            ViewBag.DriverId = new SelectList(_context.Driver_Table, "DriverId", "DriverName", user.DriverId);
            return View(user);
        }

        // POST: Admin/EditUser/5
         // Ensure BCrypt is used for password hashing

[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUsers(int id, [Bind("UserId,Name,Email,PhoneNo,Auth_Method,IsVerified,Role,CompanyID,DriverId,Password")] User user)
    {
        if (id != user.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Fetch the existing user from the database
                var existingUser = await _context.User_Table.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Update user fields
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.PhoneNo = user.PhoneNo;
                existingUser.Auth_Method = user.Auth_Method;
                existingUser.IsVerified = user.IsVerified;
                existingUser.Role = user.Role;
                existingUser.CompanyID = user.CompanyID;
                existingUser.DriverId = user.DriverId;
                existingUser.Updated_At = DateTime.UtcNow;

                // ✅ Update the password only if a new one is provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                // ✅ Ensure correct redirection to ActiveUsers after a successful update
                return RedirectToAction("ActiveUsers", "Admin");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.User_Table.Any(u => u.UserId == user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
            ViewBag.Role = new SelectList(Enum.GetValues(typeof(UserRole)));
            // Reload dropdown data if ModelState is invalid
            ViewBag.CompanyID = new SelectList(_context.Company_Table, "CompanyId", "CompanyName", user.CompanyID);
        ViewBag.DriverId = new SelectList(_context.Driver_Table, "DriverId", "DriverName", user.DriverId);

        return View(user); // If validation fails, reload EditUsers page
    }

    // GET: Admin/DeleteUser/5
    public async Task<IActionResult> DeleteUsers(int id)
        {

            var user = await _context.User_Table
                .Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUsers")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _context.User_Table.FindAsync(id);
            if (user != null)
            {
                _context.User_Table.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ActiveUsers));
        }

        private bool UserExists(int id)
        {
            return _context.User_Table.Any(u => u.UserId == id);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var admin = await _context.User_Table
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId && u.Role == UserRole.Admin);

            if (admin == null)
            {
                return NotFound("Admin profile not found.");
            }

            return View(admin);
        }

        // ✅ POST: Update Admin Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var admin = await _context.User_Table.FindAsync(int.Parse(userId));
            if (admin == null)
            {
                return NotFound("Admin not found.");
            }

            // ✅ Update allowed fields
            admin.Name = model.Name;
            admin.Email = model.Email;
            admin.PhoneNo = model.PhoneNo;
            admin.Updated_At = DateTime.UtcNow;

            _context.Update(admin);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // ✅ POST: Change Admin Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var admin = await _context.User_Table.FindAsync(int.Parse(userId));
            if (admin == null)
            {
                return NotFound("Admin not found.");
            }

            // ✅ Verify Current Password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.Password))
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View("Profile", admin);
            }

            // ✅ Check if New Password Matches Confirmation
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New passwords do not match.");
                return View("Profile", admin);
            }

            // ✅ Hash and Save New Password
            admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Update(admin);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";
            return RedirectToAction(nameof(Profile));
        }
    

}


}




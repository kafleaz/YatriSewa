using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using YatriSewa.Migrations;
using YatriSewa.Models;
using YatriSewa.Services;
using Stripe;
using TokenService = YatriSewa.Services.TokenService;
using PaymentMethod = YatriSewa.Models.PaymentMethod;
using System.Net.Sockets;


namespace YatriSewa.Controllers
{
    //[Authorize(Roles = "Passenger")]
    public class PassengerController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<PassengerController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        // Modify the constructor to accept ILogger<OperatorController>
        public PassengerController(ApplicationContext context, ILogger<PassengerController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;  // Assign logger to the private field
            _webHostEnvironment = webHostEnvironment;
        }

        private string GenerateTicketNumber()
        {
            return $"TKT-{DateTime.UtcNow:yyMMddHHmm}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        public IActionResult PassengerDashboard()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> OperatorRequest()
        {
            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Look for an existing operator request by the current user
            var operatorRequest = await _context.Company_Table
                .FirstOrDefaultAsync(b => b.UserId == int.Parse(userId));

            if (operatorRequest == null)
            {
                // Initialize all required fields with placeholder or default values
                operatorRequest = new BusCompany
                {
                    CompanyName = string.Empty, // Placeholder value
                    CompanyAddress = string.Empty, // Placeholder value
                    ContactInfo = string.Empty, // Placeholder value
                    Reg_No = string.Empty, // Placeholder value
                    VAT_PAN = string.Empty, // Placeholder value
                    VAT_PAN_PhotoPath = null // No default path
                };
            }

            return View(operatorRequest); // Pass the existing request or a new model to the view
        }


        [HttpGet]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> DriverRequest()
        {
            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            // Look for an existing driver request by the current user
            var driverRequest = await _context.Driver_Table
                .FirstOrDefaultAsync(d => d.UserId == int.Parse(userId));

            if (driverRequest == null)
            {
                // Initialize all required fields
                driverRequest = new BusDriver
                {
                    DriverName = string.Empty, // Placeholder or initial value
                    LicenseNumber = string.Empty, // Placeholder or initial value
                    PhoneNumber = string.Empty, // Placeholder or initial value
                    Address = string.Empty, // Placeholder or initial value
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
                    // Default value, adjust as needed
                };
            }

            return View(driverRequest); // Pass the existing request or a new model to the view
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitDriverRequest(BusDriver model)
        {
            if (!ModelState.IsValid)
            {
                return View("DriverRequest", model);
            }

            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }

            model.UserId = int.Parse(userId);

            // Check if the user already has a driver request
            var existingRequest = await _context.Driver_Table
                .FirstOrDefaultAsync(d => d.UserId == model.UserId);

            // Handle file upload for LicensePhotoPath
            var licensePhoto = Request.Form.Files.FirstOrDefault();
            if (licensePhoto != null && licensePhoto.Length > 0)
            {
                // Delete the old license photo if it exists
                if (existingRequest?.LicensePhotoPath != null)
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingRequest.LicensePhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Define the upload path
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "licensephoto");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(licensePhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await licensePhoto.CopyToAsync(fileStream);
                }

                // Update the model's LicensePhotoPath with the relative path
                model.LicensePhotoPath = "/licensephoto/" + uniqueFileName;
            }

            if (existingRequest == null)
            {
                // New request submission
                model.CompanyId = null;
                model.IsAvailable = false;
                model.IsAssigned = false;

                _context.Driver_Table.Add(model);
            }
            else
            {
                // Update existing request fields
                existingRequest.DriverName = model.DriverName;
                existingRequest.LicenseNumber = model.LicenseNumber;
                existingRequest.PhoneNumber = model.PhoneNumber;
                existingRequest.Address = model.Address;
                existingRequest.DateOfBirth = model.DateOfBirth;
                existingRequest.LicensePhotoPath = model.LicensePhotoPath ?? existingRequest.LicensePhotoPath; // Keep old photo if new one is not uploaded
                existingRequest.IsAvailable = model.IsAvailable;

                _context.Driver_Table.Update(existingRequest);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("HomePage", "Passenger");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database save: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the data.");
                return View("DriverRequest", model);
            }
        }






        [HttpPost]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> SubmitOperatorRequest(BusCompany model)
        {
            if (ModelState.IsValid)
            {
                // Get the current user ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(); // Handle invalid or missing user ID
                }
                model.UserId = int.Parse(userId);
                var vatPanPhoto = Request.Form.Files.FirstOrDefault();
                if (vatPanPhoto != null && vatPanPhoto.Length > 0)
                {
                    // Define the upload path
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "vat_pan_photo");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(vatPanPhoto.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the server
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vatPanPhoto.CopyToAsync(fileStream);
                    }
                    model.VAT_PAN_PhotoPath = "/vat_pan_photo/" + uniqueFileName;
                }

                // Save the BusCompany details to the database
                _context.Company_Table.Add(model);
                await _context.SaveChangesAsync();

                // Redirect to a confirmation or a list page after successful submission
                return RedirectToAction("HomePage", "Passenger");
            }

            // If model state is not valid, return to the form with validation errors
            return View("OperatorRequest", model);
        }
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Handle invalid or missing user ID
            }
            // Fetch user details from the database

            var user = _context.User_Table.FirstOrDefault(u => u.UserId == int.Parse(userId));
            if (user == null)
            {
                return NotFound(); // Handle case where user doesn't exist
            }

            return View(user); // Pass the user model to the view
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            // Get the logged-in user's ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(); // User is not logged in or ID is invalid
            }

            // Fetch the user from the database
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Update fields
            user.Name = model.Name;

            // Save changes
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }



        [HttpPost]
        public async Task<IActionResult> UpdateProfilePhoto(IFormFile ProfilePic)
        {
            // Get the logged-in user's ID from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var loggedInUserId))
            {
                return Unauthorized();
            }

            // Fetch the user's record
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.UserId == loggedInUserId);
            if (user == null)
            {
                return NotFound();
            }

            // Handle Profile Picture
            if (ProfilePic != null)
            {
                // Ensure the directory exists
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProfilePhoto");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete the old photo if available
                if (!string.IsNullOrEmpty(user.ProfilePicPath))
                {
                    var oldPicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPicPath))
                    {
                        System.IO.File.Delete(oldPicPath);
                    }
                }

                // Save the new photo
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePic.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePic.CopyToAsync(fileStream);
                }

                // Update the user's profile picture path
                user.ProfilePicPath = "/ProfilePhoto/" + uniqueFileName;

                // Save changes
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Profile picture updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
        public IActionResult HomePage()
        {
            // Retrieve all routes from the database
            var routes = _context.Route_Table
                .AsEnumerable() // Switch to client-side evaluation
                .SelectMany(r => new[] { r.StartLocation, r.EndLocation })
                .Distinct()
                .OrderBy(location => location) // Optional: Alphabetical order
                .ToList();

            ViewBag.Locations = routes;

            // Fetch the profile picture for the logged-in user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Adjust based on authentication

            // Fetch the user object based on the claim
            var user = _context.User_Table.FirstOrDefault(u => u.UserId.ToString() == userIdClaim);

            // Set the profile picture path in ViewBag
            ViewBag.ProfilePicPath = user?.ProfilePicPath ?? Url.Content("~/img/default-profile.png");

            return View();
        }

        [HttpGet]
        public IActionResult BusListing()
        {
            // Retrieve search parameters from TempData
            ViewBag.From = TempData["From"]?.ToString() ?? string.Empty;
            ViewBag.To = TempData["To"]?.ToString() ?? string.Empty;
            ViewBag.Date = TempData["Date"]?.ToString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> BusListing(string from, string to, DateTime date)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || date == default)
            {
                ViewBag.ErrorMessage = "Please fill out all fields.";
                return View("Home");
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var schedules = await _context.Schedule_Table
                .Include(s => s.Route)
                .Include(s => s.Bus)
                    .ThenInclude(b => b.Service) // Include Service details
                .Where(s =>
                    s.Route.StartLocation == from &&
                    s.Route.EndLocation == to &&
                    s.DepartureTime.Date == date.Date &&
                    s.AvailableSeats > 0)
                .ToListAsync();

                TempData["From"] = from;
                TempData["To"] = to;
                TempData["Date"] = date.ToString("yyyy-MM-dd");

            return View("BusListing", schedules);
        }

        [HttpGet]
        public async Task<IActionResult> BusDetails(string token)
        {
            try
            {
                var tokenService = new TokenService();

                // Decode the token to get BusId
                var tokenData = tokenService.DecodeToken(token);
                int busId = tokenData.BusId;

                // Fetch the bus details
                var bus = await _context.Bus_Table
                    .Include(b => b.Service)
                    .Include(b => b.Schedules)
                    .ThenInclude(s => s.Route)
                    .FirstOrDefaultAsync(b => b.BusId == busId);

                if (bus == null)
                    return NotFound();
                // Preserve search parameters in TempData for back navigation
                TempData["From"] = TempData["From"];
                TempData["To"] = TempData["To"];
                TempData["Date"] = TempData["Date"];

                var journeyDate = bus.Schedules?.FirstOrDefault()?.DepartureTime.Date ?? DateTime.UtcNow.Date;

                var viewModel = new BusDetailsViewModel
                {
                    BusId = bus.BusId,
                    JourneyDate = journeyDate,
                    BusName = bus.BusName,
                    BusNumber = bus.BusNumber,
                    Description = bus.Description,
                    ImagePath = bus.ImagePath ?? "default-bus.jpg",
                    StartLocation = bus.Schedules?.FirstOrDefault()?.Route?.StartLocation ?? "N/A",
                    EndLocation = bus.Schedules?.FirstOrDefault()?.Route?.EndLocation ?? "N/A",
                    Price = bus.Schedules?.FirstOrDefault()?.Price ?? 0,
                    DepartureTime = bus.Schedules?.FirstOrDefault()?.DepartureTime,
                    ArrivalTime = bus.Schedules?.FirstOrDefault()?.ArrivalTime,
                    Wifi = bus.Service?.Wifi ?? false,
                    AC = bus.Service?.AC ?? false,
                    SafetyFeatures = bus.Service?.SafetyFeatures ?? "N/A",
                    Essentials = bus.Service?.Essentials ?? "N/A",
                    Snacks = bus.Service?.Snacks ?? "N/A",
                    Reviews = new List<string> { "Great Bus!", "Comfortable journey!" } // Example static reviews
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BusDetails: {ex.Message}");
                return BadRequest("Invalid token.");
            }
        }



        public async Task ClearExpiredReservations()
        {
            var expirationTime = DateTime.UtcNow.AddMinutes(-20); // 20 minutes expiration
            var expiredSeats = await _context.Seat_Table
                .Where(s => s.Status == SeatStatus.Reserved && s.ReservedAt < expirationTime)
                .ToListAsync();

            //foreach (var seat in expiredSeats)
            //{
            //    seat.Status = SeatStatus.Available;
            //    seat.ReservedByUserId = null;
            //    seat.ReservedAt = null;
            //}
            _context.Seat_Table.RemoveRange(expiredSeats);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> SeatSelection(string token)
        {
            try
            {
                var tokenService = new TokenService();

                // Decode the token to extract busId and journeyDate
                var tokenData = tokenService.DecodeToken(token);
                int busId = tokenData.BusId;
                DateTime journeyDate = tokenData.JourneyDate;

                // Fetch the schedule details along with the bus details
                var schedule = await _context.Schedule_Table
                    .Include(s => s.Bus)
                    .ThenInclude(b => b.Seats)
                    .Include(s => s.Bus.Service)
                    .FirstOrDefaultAsync(s => s.BusId == busId && s.DepartureTime.Date == journeyDate.Date);

                if (schedule == null)
                {
                    return NotFound("Schedule not found.");
                }

                var bus = schedule.Bus;

                if (bus == null)
                {
                    return NotFound("Bus not found.");
                }

                // Use schedule price if available; otherwise, fallback to bus price
                var price = schedule.Price > 0 ? schedule.Price : bus.Price;

                // Get the reserved seats for the logged-in user

#pragma warning disable CS8604 // Possible null reference argument.
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var reservedSeats = bus.Seats
                    .Where(s => s.ReservedByUserId == userId && s.Status == SeatStatus.Reserved)
                    .Select(s => s.SeatNumber)
                    .ToList();

                // Populate the view model with essential data
                var viewModel = new BusDetailsViewModel
                {
                    BusId = bus.BusId,
                    ScheduleId = schedule.ScheduleId,
                    BusName = bus.BusName,
                    BusNumber = bus.BusNumber,
                    Price = price,
                    StartLocation = schedule.Route?.StartLocation,
                    EndLocation = schedule.Route?.EndLocation,
                    JourneyDate = journeyDate,
                    Seats = bus.Seats.ToList(),
                    Wifi = bus.Service?.Wifi ?? false,
                    AC = bus.Service?.AC ?? false,
                    SafetyFeatures = bus.Service?.SafetyFeatures ?? "N/A",
                    Essentials = bus.Service?.Essentials ?? "N/A",
                    Snacks = bus.Service?.Snacks ?? "N/A",
                    SeatType = bus.Service?.SeatType ?? SeatType.iixii, // Default to 2x2
                    SeatCapacity = bus.SeatCapacity,
                    ReservedSeats = reservedSeats
                };
                Console.WriteLine($"Reserved Seats: {string.Join(", ", reservedSeats)}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SeatSelection: {ex.Message}");
                return BadRequest("Invalid token.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        {
            try
            {
                Console.WriteLine($"Received BusId: {request.BusId}");
                Console.WriteLine($"Seat Numbers: {string.Join(",", request.SeatNumbers)}");

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var alreadyReservedSeats = await _context.Seat_Table
                    .Where(s => s.ReservedByUserId == userId && s.Status == SeatStatus.Reserved)
                    .CountAsync();

                if (alreadyReservedSeats + request.SeatNumbers.Count > 6)
                {
                    return Json(new { success = false, message = "You cannot reserve more than 6 seats in total." });
                }

                if (request.BusId <= 0 || request.SeatNumbers == null || request.SeatNumbers.Count == 0)
                {
                    return Json(new { success = false, message = "Invalid request. Please try again." });
                }
                
                // Fetch the bus and seats stored in the database
                var existingSeats = await _context.Seat_Table
                    .Where(s => s.BusId == request.BusId && request.SeatNumbers.Contains(s.SeatNumber))
                    .ToListAsync();

                // Determine unavailable seats (already reserved or booked)
                var unavailableSeats = existingSeats
                    .Where(s => s.Status != SeatStatus.Available)
                    .Select(s => s.SeatNumber)
                    .ToList();

                if (unavailableSeats.Any())
                {
                    return Json(new { success = false, message = $"Some seats are already reserved or booked: {string.Join(", ", unavailableSeats)}." });
                }

                // Reserve requested seats: Add new entries for seats not in the database
                foreach (var seatNumber in request.SeatNumbers)
                {
                    var existingSeat = existingSeats.FirstOrDefault(s => s.SeatNumber == seatNumber);

                    if (existingSeat == null) // Seat is "Available" (not stored in the database)
                    {

                        var newSeat = new Seat
                        {
                            SeatNumber = seatNumber,
                            BusId = request.BusId,
                            Status = SeatStatus.Reserved,
                            ReservedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                            ReservedAt = DateTime.UtcNow
                        };

                        _context.Seat_Table.Add(newSeat);
                    }
                    else // Seat exists, update its status to "Reserved"
                    {
                        existingSeat.Status = SeatStatus.Reserved;

                        existingSeat.ReservedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                        existingSeat.ReservedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Seats reserved successfully." });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in ReserveSeats: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while reserving seats. Please try again later." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation([FromBody] ReserveSeatsRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var reservedSeats = await _context.Seat_Table
                .Where(s => s.ReservedByUserId == userId && s.BusId == request.BusId)
                .ToListAsync();

            if (!reservedSeats.Any())
            {
                return Json(new { success = false, message = "You have no reserved seats to cancel." });
            }

            foreach (var seat in reservedSeats)
            {
                seat.Status = SeatStatus.Available;
                seat.ReservedByUserId = null;
                seat.ReservedAt = null;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Reservation canceled successfully." });
        }


        [HttpPost]
        public async Task<IActionResult> BuyReservedSeats([FromBody] ReserveSeatsRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var reservedSeats = await _context.Seat_Table
                .Where(s => s.ReservedByUserId == userId && s.BusId == request.BusId && s.Status == SeatStatus.Reserved)
                .ToListAsync();

            if (!reservedSeats.Any())
            {
                return Json(new { success = false, message = "You have no reserved seats to buy." });
            }

            var schedule = await _context.Schedule_Table.FirstOrDefaultAsync(s => s.BusId == request.BusId);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Schedule not found." });
            }

            var seatPrice = schedule.Price;
            var totalAmount = reservedSeats.Count * seatPrice;

            // Generate token
            var tokenService = new TokenService();
            var tokenData = new TokenData
            {
                TokenType = "BuyReservedSeats",
                BusId = request.BusId,
                TotalAmount = totalAmount,
                ReservedSeatIds = reservedSeats.Select(s => s.SeatId).ToList()
            };
            var token = tokenService.GenerateToken(tokenData);

            return Json(new { success = true, redirectUrl = $"/Passenger/Payment?token={token}" });
        }

#pragma warning restore CS8604 // Possible null reference argument.

        [HttpPost]
        public async Task<IActionResult> BuySeats([FromBody] ReserveSeatsRequest request)
        {
            try
            {
                if (request.SeatNumbers == null || request.SeatNumbers.Count == 0)
                {
                    return Json(new { success = false, message = "No seats selected for purchase." });
                }

                var schedule = await _context.Schedule_Table
                    .FirstOrDefaultAsync(s => s.ScheduleId == request.ScheduleId);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "Schedule not found." });
                }

                var seatPrice = schedule.Price;
                var totalAmount = request.SeatNumbers.Count * seatPrice;

                // Create an instance of TokenData
                var tokenData = new TokenData
                {
                    TokenType = "BuySeats",
                    ScheduleId = request.ScheduleId,
                    BusId = request.BusId,
                    SeatNumbers = request.SeatNumbers, // Ensure this matches the property type in TokenData
                    TotalAmount = totalAmount
                };

                // Generate the token
                var tokenService = new TokenService();
                string token = tokenService.GenerateToken(tokenData);

                // Redirect to payment page with the token
                return Json(new
                {
                    success = true,
                    redirectUrl = $"/Passenger/Payment?token={token}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BuySeats: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }


        [HttpGet]
        [HttpGet]
        public IActionResult Payment(string token, bool paymentSuccess = false)
        {
            try
            {
                // Get the UserId from the claims
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == 0)
                {
                    return Unauthorized("User session has expired. Please log in again.");
                }

                var tokenService = new TokenService();
                var data = tokenService.DecodeToken(token);

                Console.WriteLine($"Token received: {token}");

                // Fetch user details from the database
                var user = _context.User_Table.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                // Fetch schedule and route details from the database
                var schedule = _context.Schedule_Table
                    .Include(s => s.Route)
                    .FirstOrDefault(s => s.ScheduleId == data.ScheduleId);
                if (schedule == null)
                {
                    return BadRequest("Schedule not found.");
                }

                string ticketNumber = GenerateTicketNumber(); // Generate a unique ticket number

                var viewModel = new PaymentViewModel
                {
                    BusId = schedule.BusId,
                    FullName = user.Name,
                    PhoneNumber = user.PhoneNo,
                    TicketNumber = ticketNumber,
                    StartLocation = schedule.Route?.StartLocation, // Fetch StartLocation from Route
                    EndLocation = schedule.Route?.EndLocation,   // Fetch EndLocation from Route
                    TotalAmount = data.TotalAmount,
                    TokenType = data.TokenType
                };

                if (data.TokenType == "BuyReservedSeats")
                {
                    viewModel.ReservedSeatIds = data.ReservedSeatIds;
                }
                else if (data.TokenType == "BuySeats")
                {
                    viewModel.SeatNumbers = data.SeatNumbers;
                }
                else
                {
                    return BadRequest("Invalid token type.");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment: {ex.Message}");
                return BadRequest("An error occurred while processing the payment.");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> ConfirmPayment(PaymentConfirmationRequest request)
        //{
        //    //try
        //    //{
        //        // Step 1: Validate input data
        //        if (request == null ||
        //            string.IsNullOrEmpty(request.PhoneNumber) ||
        //            string.IsNullOrEmpty(request.FullName) ||
        //            request.SeatNumbers == null ||
        //            !request.SeatNumbers.Any())
        //        {
        //            //return BadRequest("Invalid payment request data.");
        //            return Json(new { success = false, message = "Invalid payment request data." });
        //        }

        //        // Step 2: Fetch or create Passenger entry
        //        //var passenger = await _context.Passenger_Table
        //        //    .FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber);

        //        //if (passenger == null)
        //        //{
        //          var passenger = new Passenger
        //            {
        //                Name = request.FullName,
        //                PhoneNumber = request.PhoneNumber,
        //                BoardingPoint = request.BoardingPoint,
        //                DroppingPoint = request.DroppingPoint
        //            };
        //            _context.Passenger_Table.Add(passenger);
        //            await _context.SaveChangesAsync();

        //    var bus = await _context.Bus_Table.FirstOrDefaultAsync(b => b.BusId == request.BusId);
        //    if (bus == null)
        //    {
        //        return BadRequest("Invalid BusId. The bus does not exist.");
        //    }
        //    // Step 3: Create a new Booking entry
        //    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    var booking = new Booking
        //        {
        //            UserId = userId,
        //            PassengerId = passenger.PassengerId,
        //            BusId = request.BusId,
        //            TotalSeats = request.SeatNumbers.Count,
        //            TotalAmount = request.TotalAmount,
        //            BookingDate = DateTime.UtcNow,
        //            Status = BookingStatus.Pending // Mark as pending until payment is processed
        //        };
        //        _context.Booking_Table.Add(booking);
        //        await _context.SaveChangesAsync();

        //        // Step 4: Create Tickets and update Seat status
        //        foreach (var seatNumber in request.SeatNumbers)
        //        {
        //            // Check if the seat exists in the Seat_Table
        //            var seat = await _context.Seat_Table
        //                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber && s.BusId == request.BusId);

        //            //if (seat == null)
        //            //{
        //            //    return BadRequest($"Seat {seatNumber} does not exist or is invalid.");
        //            //}

        //            if (seat.Status != SeatStatus.Reserved)
        //            {
        //                return BadRequest($"Seat {seatNumber} is not reserved and cannot be booked.");
        //            }

        //            // Update seat status to Booked
        //            seat.Status = SeatStatus.Booked;

        //            // Create a ticket for the booking
        //            var ticket = new Ticket
        //            {
        //                BookingId = booking.BookingId,
        //                SeatId = seat.SeatId,
        //                TicketNo = request.TicketNumber,
        //                Price = request.PricePerSeat
        //            };
        //            _context.Ticket_Table.Add(ticket);
        //        }

        //        await _context.SaveChangesAsync();

        //        // Step 5: Redirect to payment card page based on payment method
        //        string redirectUrl = request.PaymentMethod switch
        //        {
        //            "Esewa" => Url.Action("Esewa", "PaymentCard",
        //                new { bookingId = booking.BookingId, totalAmount = request.TotalAmount }),
        //            "Stripe" => Url.Action("Stripe", "PaymentCard",
        //                new { bookingId = booking.BookingId, totalAmount = request.TotalAmount }),
        //            _ => null
        //        };

        //        if (redirectUrl == null)
        //        {
        //            return BadRequest("Unsupported payment method selected.");
        //        }

        //        return Redirect(redirectUrl);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Console.WriteLine($"Error in ConfirmPayment: {ex.Message}");
        //    //    //return BadRequest("An error occurred while processing your request.");
        //    //    return Json(new { success = false, message = "An error occurred while processing your request." });
        //    //}
        //}

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentConfirmationRequest request)
        {
            try
            {
                // Step 1: Validate input
                if (request == null ||
                    string.IsNullOrEmpty(request.PhoneNumber) ||
                    string.IsNullOrEmpty(request.FullName) ||
                    string.IsNullOrEmpty(request.SeatNumbers))
                {
                    return Json(new { success = false, message = "Invalid payment request data." });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Step 2: Split seat numbers into a list
                var seatNumbers = request.SeatNumbers.Split(',').Select(s => s.Trim()).ToList();

                if (!seatNumbers.Any())
                {
                    return Json(new { success = false, message = "No seat numbers provided." });
                }

                // Fetch or create passenger
                var passenger = await _context.Passenger_Table
                    .FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber);
                //if (passenger == null)
                //{
                    passenger = new Passenger
                    {
                        Name = request.FullName,
                        PhoneNumber = request.PhoneNumber,
                        BoardingPoint = request.BoardingPoint,
                        DroppingPoint = request.DroppingPoint
                    };
                    _context.Passenger_Table.Add(passenger);
                    await _context.SaveChangesAsync();
                //}

                // Create a new booking
                var booking = new Booking
                {
                    UserId = userId,
                    PassengerId = passenger.PassengerId,
                    BusId = request.BusId,
                    TotalSeats = seatNumbers.Count,
                    TotalAmount = request.TotalAmount,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending
                };
                _context.Booking_Table.Add(booking);
                await _context.SaveChangesAsync();

                // Fetch schedule and seat price
                var schedule = await _context.Schedule_Table.FirstOrDefaultAsync(s => s.BusId == request.BusId);
                if (schedule == null)
                {
                    return Json(new { success = false, message = "Schedule not found for the selected bus." });
                }
                decimal pricePerSeat = schedule.Price;
                foreach (var seatNumber in seatNumbers)
                {
                    var seat = await _context.Seat_Table
                        .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber && s.BusId == request.BusId);

                    if (seat == null)
                    {
                        seat = new Seat
                        {
                            SeatNumber = seatNumber,
                            BusId = request.BusId,
                            Status = SeatStatus.Available,
                            BookingId = booking.BookingId
                        };
                        _context.Seat_Table.Add(seat);
                    }
                    else if (seat.Status == SeatStatus.Available ||
                             (seat.Status == SeatStatus.Reserved && seat.ReservedByUserId == userId))
                    {
                        seat.BookingId = booking.BookingId;
                    }
                    else
                    {
                        return BadRequest($"Seat {seatNumber} is already booked or not available for purchase.");
                    }
                }
                await _context.SaveChangesAsync();


                // Redirect based on payment method
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string redirectUrl = request.PaymentMethod switch
                {
                    "Esewa" => Url.Action("EsewaPayment", "Passenger",
                        new { bookingId = booking.BookingId, totalAmount = request.TotalAmount }),
                    "Stripe" => Url.Action("StripePaymentCard", "Passenger",
                        new { bookingId = booking.BookingId, scheduleId = schedule.ScheduleId, totalAmount = request.TotalAmount }),
                    _ => null
                };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                if (redirectUrl == null)
                {
                    return Json(new { success = false, message = "Unsupported payment method selected." });
                }

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmPayment: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EsewaPayment(int bookingId, decimal totalAmount)
        {
            // Fetch the booking along with associated bus and company information
            var booking = await _context.Booking_Table
                .Include(b => b.Bus)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.Bus == null)
            {
                return BadRequest("Invalid booking ID or bus information not found.");
            }

            // Determine operatorId (CompanyId or DriverId) based on the booking
            //var operatorId = booking.Bus.CompanyId.HasValue ? booking.Bus.CompanyId.Value : booking.Bus.DriverId;
            var operatorId = booking.Bus.CompanyId != 0 ? booking.Bus.CompanyId : booking.Bus.DriverId;


            if (!operatorId.HasValue)
            {
                return BadRequest("Operator information not found for the booking.");
            }

            // Fetch merchant data using operatorId
            var merchant = await _context.Merchant_Table
                .FirstOrDefaultAsync(m => m.CompanyId == operatorId || m.DriverId == operatorId);

            if (merchant == null)
            {
                return BadRequest("Merchant information not found.");
            }

            /*var esewaPaymentUrl = "https://uat.esewa.com.np/epay/main";*/ // Test environment URL
            var esewaPaymentUrl = "https://rc.esewa.com.np/epay/main";
            // Generate absolute success and failure URLs
            var successUrl = Url.Action("EsewaSuccess", "Passenger", new { bookingId, totalAmount }, Request.Scheme, Request.Host.Value);
            var failureUrl = Url.Action("EsewaFailure", "Passenger", new { bookingId }, Request.Scheme, Request.Host.Value);

            // Prepare eSewa payment request data
            var esewaRequestData = new Dictionary<string, string>
            {
                { "amt", totalAmount.ToString("F2") }, // Base amount
                { "tAmt", (totalAmount + merchant.TaxAmount + merchant.ServiceCharge + merchant.ProductCharge).ToString("F2") }, // Total amount
                { "txAmt", merchant.TaxAmount.ToString("F2") }, // Tax amount
                { "psc", merchant.ServiceCharge.ToString("F2") }, // Service charge
                { "pdc", merchant.ProductCharge.ToString("F2") }, // Product charge
                { "scd", merchant.MerchantCode }, // Merchant code
                { "pid", $"booking-{bookingId}" }, // Unique payment identifier
                { "su", successUrl }, // Success URL
                { "fu", failureUrl } // Failure URL
            };

            // Redirect to eSewa payment gateway
            var esewaRedirectUrl = QueryHelpers.AddQueryString(esewaPaymentUrl, esewaRequestData);
            return Redirect(esewaRedirectUrl);
        }




        [HttpGet]
        public async Task<IActionResult> EsewaSuccess(int bookingId, string refId, decimal totalAmount)
        {
            var esewaVerificationUrl = "https://rc.esewa.com.np/epay/transrec"; // Test environment URL

            // Fetch booking and related bus
            var booking = await _context.Booking_Table
                .Include(b => b.Bus) // Include related bus
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return BadRequest("Booking not found.");
            }

            // Fetch seats related to this booking indirectly
            var seats = await _context.Seat_Table
                .Where(s => s.BookingId == bookingId)
                .ToListAsync();

            if (!seats.Any())
            {
                return BadRequest("No seats found for this booking.");
            }

            // Fetch dynamic merchant code based on operator
            int operatorId = booking.Bus?.CompanyId ?? booking.Bus?.DriverId ?? 0;
            var merchant = await _context.Merchant_Table
                .FirstOrDefaultAsync(m => m.CompanyId == operatorId || m.DriverId == operatorId);

            if (merchant == null)
            {
                return BadRequest("Merchant information not found.");
            }

            var merchantCode = merchant.MerchantCode;
            Console.WriteLine($"Merchant code: {merchantCode}");

            // Prepare eSewa verification request
                    var esewaVerificationData = new Dictionary<string, string>

            {

                { "amt", totalAmount.ToString("F2") },
                { "scd", merchantCode },
                { "pid", $"booking-{bookingId}" },
                { "rid", refId }
            };
            // Verify payment with eSewa
            // Log verification data
            foreach (var kvp in esewaVerificationData)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(esewaVerificationData);
                var response = await httpClient.PostAsync(esewaVerificationUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    //if (responseString.Contains("<response_code>Success</response_code>", StringComparison.OrdinalIgnoreCase))

                        if (responseString.Contains("<response_code>Success</response_code>"))
                        Console.WriteLine($"eSewa verification response: {responseString}");
                    {

                        // Update booking status to Paid
                        booking.Status = BookingStatus.Paid;

                        // Update seat statuses
                        foreach (var seat in seats)
                        {
                            seat.Status = SeatStatus.Booked;
                        }

                        // Generate tickets
                        foreach (var seat in seats)
                        {
                            var ticket = new Ticket
                            {
                                BookingId = booking.BookingId,
                                SeatId = seat.SeatId,
                                
                                Price = totalAmount / booking.TotalSeats.Value,
                                TicketNo = Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper(),
                                PNR = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                            };
                            _context.Ticket_Table.Add(ticket);
                        }

                        // Log transaction in EsewaTransaction table
                        var esewaTransaction = new EsewaTransaction
                        {
                            MerchantCode = merchantCode,
                            PaymentId = null,
                            TotalAmount = totalAmount,
                            TaxAmount = merchant.TaxAmount,
                            ServiceCharge = merchant.ServiceCharge,
                            ProductCharge = merchant.ProductCharge,
                            PaidAmount = totalAmount,
                            ReferenceId = refId.ToString(),
                            BookingId = bookingId.ToString(),
                            Status = "Successful",
                            TransactionDate = DateTime.UtcNow
                        };
                        _context.EsewaTransaction_Table.Add(esewaTransaction);
                        await _context.SaveChangesAsync();

                        // Log payment in Payment table with PassengerId
                        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        var payment = new Payment
                        {
                            UserId = userId,
                            PassengerId = booking.PassengerId,
                            BookingId = bookingId,
                            AmountPaid = totalAmount,
                            PaymentDate = DateTime.UtcNow,
                            PaymentMethod = PaymentMethod.Esewa,
                            Status = PaymentStatus.Successful,
                            TransactionId = esewaTransaction.TransactionId // Foreign key to EsewaTransaction
                        };
                        _context.Payment_Table.Add(payment);
                        await _context.SaveChangesAsync();

                        esewaTransaction.PaymentId = payment.PaymentId.ToString();
                        _context.EsewaTransaction_Table.Update(esewaTransaction);

                        await _context.SaveChangesAsync();

                        //return RedirectToAction("Ticket", "Passenger", new { bookingId });
                        return RedirectToAction("Ticket", "Passenger", new { bookingId, paymentSuccess = true });
                    }
                }
            }

            return RedirectToAction("Payment", "Passenger", new { bookingId, paymentSuccess = false });
        }


        [HttpGet]
        public IActionResult EsewaFailure(int bookingId)
        {
            // Log the failure for debugging purposes
            Console.WriteLine($"Esewa payment failed for Booking ID: {bookingId}");

            // Display an error message or redirect to an appropriate page
            return View("EsewaFailure", new { BookingId = bookingId });
        }


        [HttpGet]
        public async Task<IActionResult> StripePaymentCard(int bookingId, int scheduleId, decimal totalAmount)
        {
            try
            {
                // Fetch booking details using bookingId
                var booking = await _context.Booking_Table
                    .Include(b => b.Bus)
                    .Include(b => b.Passenger)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("ErrorPage");
                }
                var schedule = await _context.Schedule_Table
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

                if (schedule == null)
                {
                    TempData["Error"] = "Schedule not found.";
                    return RedirectToAction("ErrorPage");
                }
                // Create a PaymentViewModel to pass necessary data to the view
                var paymentViewModel = new PaymentViewModel
                {
                    BusId = booking.BusId,
                    BusName=booking.Bus.BusName,
                    PickupTime = schedule.DepartureTime.ToString("hh:mm tt"),
                    PassengerId = booking.Passenger.PassengerId,
                    FullName = booking.Passenger.Name,
                    StartLocation = booking.Passenger.BoardingPoint,
                    EndLocation = booking.Passenger.DroppingPoint,
                    SeatNumbers =  _context.Seat_Table
                    .Where(s => s.BookingId == booking.BookingId)
                    .Select(s => s.SeatNumber)
                    .ToList(),
                    TotalAmount = totalAmount
                };
                ViewBag.BookingId = bookingId;
                return View(paymentViewModel); // Render the Stripe payment card view
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StripePaymentCard: {ex.Message}");
                TempData["Error"] = "An error occurred while processing your request.";
                return RedirectToAction("ErrorPage");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> StripePaymentSuccess(string stripeToken, int bookingId, decimal totalAmount)
        //{
        //    try
        //    {
        //        StripeConfiguration.ApiKey = "sk_test_51PNIqUB3UNeeoGIf1BwZwOzVjkkGUopjyJgVaTeP57NJhJNNqZNjFLfzQgK1W5kMinCZnaTN8iugH3qLXlxEgcgm00VBFEoCPp";

        //        var paymentIntentOptions = new PaymentIntentCreateOptions
        //        {
        //            Amount = (long)(totalAmount * 100), // Convert to cents
        //            Currency = "npr",
        //            PaymentMethod = stripeToken,
        //            Confirm = true
        //        };

        //        var paymentIntentService = new PaymentIntentService();
        //        PaymentIntent paymentIntent = paymentIntentService.Create(paymentIntentOptions);

        //        if (paymentIntent.Status == "succeeded")
        //        {
        //            // ✅ Retrieve the charge from PaymentIntent
        //            var chargeId = paymentIntent.LatestChargeId; // Get charge ID

        //            await ProcessPaymentSuccess(bookingId, totalAmount, paymentIntent.Id, chargeId);
        //            return RedirectToAction("TicketDetails", "Passenger", new { bookingId, paymentSuccess = true });
        //        }
        //        else
        //        {
        //            return BadRequest("Payment failed.");
        //        }
        //    }
        //    catch (StripeException ex)
        //    {
        //        Console.WriteLine($"Stripe error: {ex.Message}");
        //        return BadRequest("An error occurred while processing the payment.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"General error: {ex.Message}");
        //        return BadRequest("An unexpected error occurred.");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> StripePaymentSuccess(string stripeToken, int bookingId, decimal totalAmount)
        {
            try
            {
                // ✅ Set Stripe API key
                StripeConfiguration.ApiKey = "sk_test_51PNIqUB3UNeeoGIf1BwZwOzVjkkGUopjyJgVaTeP57NJhJNNqZNjFLfzQgK1W5kMinCZnaTN8iugH3qLXlxEgcgm00VBFEoCPp";

                // ✅ Create a charge request
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(totalAmount * 100), // Convert to cents
                    Currency = "npr",
                    Description = "Bus Ticket Payment",
                    Source = stripeToken, // Token from frontend
                    Metadata = new Dictionary<string, string>
            {
                { "BookingId", bookingId.ToString() }
            }
                };

                var service = new ChargeService();
                var charge = service.Create(options);

                // ✅ Check if the payment succeeded
                if (charge.Status == "succeeded")
                {
                    // ✅ Process the booking, ticket, and payment logic
                    await ProcessPaymentSuccess(bookingId, totalAmount, charge.Id);

                    // ✅ Redirect to ticket page after successful payment
                    return RedirectToAction("Ticket", "Passenger", new { bookingId, paymentSuccess = true });
                }
                else
                {
                    Console.WriteLine("❌ Payment failed with status: " + charge.Status);
                    return BadRequest("Payment failed. Status: " + charge.Status);
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine("❌ Stripe API Error: " + ex.Message);
                return BadRequest("Stripe error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ General Error: " + ex.Message);
                return BadRequest("An unexpected error occurred: " + ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> StripePaymentSuccess(string stripeToken, int bookingId, decimal totalAmount)
        //{
        //    //try
        //    //{
        //        // Ensure API key is set
        //        StripeConfiguration.ApiKey = "sk_test_51PNIqUB3UNeeoGIf1BwZwOzVjkkGUopjyJgVaTeP57NJhJNNqZNjFLfzQgK1W5kMinCZnaTN8iugH3qLXlxEgcgm00VBFEoCPp";

        //    // Create a charge
        //    //var options = new ChargeCreateOptions
        //    //{
        //    //    Amount = (long)(totalAmount * 100), // Convert to cents
        //    //    Currency = "npr",
        //    //    Description = "Bus Ticket Payment",
        //    //    Source = stripeToken, // Token from the frontend
        //    //    Metadata = new Dictionary<string, string>
        //    //{
        //    //{ "BookingId", bookingId.ToString() }
        //    //}
        //    //};

        //    //var service = new ChargeService();
        //    //var charge = service.Create(options);

        //    //if (charge.Status == "succeeded")
        //    //{
        //    //    // Process booking, ticket, and payment logic
        //    //    await ProcessPaymentSuccess(bookingId, totalAmount, charge.Id);

        //    ////return RedirectToAction("Ticket", "Passenger", new { bookingId, paymentSuccess = true });
        //    //return RedirectToAction("Ticket", "Passenger", new { bookingId, paymentSuccess = true });
        //    //}
        //    //else
        //    //{
        //    //    return BadRequest("Payment failed.");
        //    //}

        //    //try
        //    //{
        //    //    StripeConfiguration.ApiKey = "sk_test_YOUR_SECRET_KEY";

        //    //    // Create a PaymentIntent instead of a Charge
        //    //    var paymentIntentOptions = new PaymentIntentCreateOptions
        //    //    {
        //    //        Amount = (long)(totalAmount * 100), // Convert to cents
        //    //        Currency = "npr",
        //    //        PaymentMethod = stripeToken, // Payment method from frontend
        //    //        Confirm = true
        //    //    };

        //    //    var paymentIntentService = new PaymentIntentService();
        //    //    PaymentIntent paymentIntent = paymentIntentService.Create(paymentIntentOptions);

        //    //    if (paymentIntent.Status == "succeeded")
        //    //    {
        //    //        await ProcessPaymentSuccess(bookingId, totalAmount, paymentIntent.Id); // ✅ Store PaymentIntent ID
        //    //        return RedirectToAction("Ticket", "Passenger", new { bookingId, paymentSuccess = true });
        //    //    }
        //    //    else
        //    //    {
        //    //        return BadRequest("Payment failed.");
        //    //    }
        //    //}
        //    //catch (StripeException ex)
        //    //{
        //    //    Console.WriteLine($"Stripe error: {ex.Message}");
        //    //    return BadRequest("An error occurred while processing the payment.");
        //    //}
        //    //}
        //    //catch (StripeException ex)
        //    //{
        //    //    Console.WriteLine($"Stripe error: {ex.Message}");
        //    //    return BadRequest("An error occurred while processing the payment.");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Console.WriteLine($"Error: {ex.Message}");
        //    //    return BadRequest("An error occurred while processing the payment.");
        //    //}
        //}

        //private async Task ProcessPaymentSuccess(int bookingId, decimal totalAmount, string stripeTransactionId)
        //{
        //    try
        //    {
        //        // Fetch the booking details
        //        var booking = await _context.Booking_Table
        //            .Include(b => b.Bus)
        //            .Include(b => b.Passenger)
        //            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        //        if (booking == null)
        //        {
        //            throw new Exception("Booking not found.");
        //        }

        //        // Fetch seats related to the booking
        //        var seats = await _context.Seat_Table
        //            .Where(s => s.BookingId == bookingId)
        //            .ToListAsync();

        //        if (!seats.Any())
        //        {
        //            throw new Exception("No seats found for this booking.");
        //        }

        //        // Update booking status to Paid
        //        booking.Status = BookingStatus.Paid;
        //        _context.Booking_Table.Update(booking);

        //        // Update seat statuses to Booked
        //        foreach (var seat in seats)
        //        {
        //            seat.Status = SeatStatus.Booked;
        //        }

        //        // Generate tickets for each seat
        //        foreach (var seat in seats)
        //        {
        //            var ticket = new Ticket
        //            {
        //                BookingId = booking.BookingId,
        //                SeatId = seat.SeatId,
        //                Price = totalAmount / booking.TotalSeats.Value, // Divide total amount by total seats
        //                TicketNo = Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper(),
        //                PNR = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
        //            };
        //            _context.Ticket_Table.Add(ticket);
        //        }

        //        var stripeTransaction = new StripeTrans
        //        {
        //            StripeTransactionId = stripeTransactionId,
        //            BookingId = bookingId,
        //            TotalAmount = totalAmount,
        //            Status = "Successed",
        //            TransactionDate = DateTime.UtcNow

        //        };
        //        _context.StripeTrans_Table.Add(stripeTransaction);
        //        await _context.SaveChangesAsync();
        //        // Log the payment in the Payment table
        //        var payment = new Payment
        //        {
        //            UserId = booking.UserId,
        //            PassengerId = booking.PassengerId,
        //            BookingId = bookingId,
        //            AmountPaid = totalAmount,
        //            PaymentDate = DateTime.UtcNow,
        //            PaymentMethod = PaymentMethod.Stripe,
        //            Status = PaymentStatus.Successful,
        //            StripeTransId = stripeTransaction.TransactionId
        //        };
        //        _context.Payment_Table.Add(payment);

        //        // Save all changes to the database
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in ProcessPaymentSuccess: {ex.Message}");
        //        throw; // Rethrow exception to handle it at a higher level
        //    }
        //}
        private async Task ProcessPaymentSuccess(int bookingId, decimal totalAmount, string chargeId)
        {
            try
            {
                var booking = await _context.Booking_Table
                    .Include(b => b.Bus)
                    .Include(b => b.Passenger)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    throw new Exception("Booking not found.");
                }

                var seats = await _context.Seat_Table
                    .Where(s => s.BookingId == bookingId)
                    .ToListAsync();

                if (!seats.Any())
                {
                    throw new Exception("No seats found for this booking.");
                }

                //  Update booking and seat statuses
                booking.Status = BookingStatus.Paid;
                _context.Booking_Table.Update(booking);

                foreach (var seat in seats)
                {
                    seat.Status = SeatStatus.Booked;
                }

                //  Generate tickets
                foreach (var seat in seats)
                {
                    var ticket = new Ticket
                    {
                        BookingId = booking.BookingId,
                        SeatId = seat.SeatId,
                        Price = totalAmount / booking.TotalSeats.Value,
                        TicketNo = Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper(),
                        PNR = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                    };
                    _context.Ticket_Table.Add(ticket);
                }

                //  Store Charge ID in StripeTrans (No PaymentIntent needed)
                var stripeTransaction = new StripeTrans
                {
                    StripeTransactionId = chargeId, // Store Charge ID
                    ChargeId = chargeId, //  Store Charge ID for reference
                    BookingId = bookingId,
                    TotalAmount = totalAmount,
                    Status = "Successed",
                    TransactionDate = DateTime.UtcNow
                };
                _context.StripeTrans_Table.Add(stripeTransaction);
                await _context.SaveChangesAsync();

                //  Save Payment with Charge ID
                var payment = new Payment
                {
                    UserId = booking.UserId,
                    PassengerId = booking.PassengerId,
                    BookingId = bookingId,
                    AmountPaid = totalAmount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = PaymentMethod.Stripe,
                    Status = PaymentStatus.Successful,
                    StripeTransId = stripeTransaction.TransactionId //  Reference StripeTrans
                };
                _context.Payment_Table.Add(payment);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPaymentSuccess: {ex.Message}");
                throw;
            }
        }




        [HttpGet]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> Ticket()
        {
            // Fetch the logged-in user's ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User not authorized.");
            }

            var tickets = await _context.Ticket_Table
                .Include(t => t.Seat)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Bus)
                        .ThenInclude(bus => bus.Route)
                .Include(t => t.Booking.Bus.Schedules)
                .Where(t => t.Booking.UserId == userId)
                .ToListAsync();

            // Transform the tickets into view models
            var ticketViewModels = tickets.Select(t => new BookingViewModel
            {
                BookingId = t.BookingId,
                BusNumber = t.Booking?.Bus?.BusNumber ?? "Unknown", // Check for null and provide a default value
                BusName = t.Booking?.Bus?.BusName ?? "Unknown Bus",
                StartLocation = t.Booking?.Bus?.Route?.StartLocation ?? "Unknown Start",
                EndLocation = t.Booking?.Bus?.Route?.EndLocation ?? "Unknown End",
                JourneyDate = t.Booking?.Bus?.Schedules.FirstOrDefault()?.DepartureTime.Date ?? DateTime.MinValue, // Use a default value for DateTime
                SelectedSeats = t.Seat.SeatNumber, // SeatId is assumed to be always present
                TotalAmount = t.Booking?.TotalAmount ?? 0 // Use 0 as a fallback for null TotalAmount
            }).ToList();
            // Pass the data to the view
            return View(ticketViewModels);
        }

        //[HttpGet]
        //public async Task<IActionResult> TicketDetails(int bookingId)
        //{
        //    // Fetch the ticket details using the ID
        //   var ticket = await _context.Ticket_Table
        //    .Include(t => t.Booking)
        //    .ThenInclude(b => b.Passenger)
        //    .Include(t => t.Booking.Bus)
        //    .FirstOrDefaultAsync(t => t.BookingId == bookingId); // Match bookingId


        //    // Fetch Schedule details using BusId
        //    var schedule = await _context.Schedule_Table
        //        .Include(s => s.Route)
        //        .FirstOrDefaultAsync(s => s.BusId == ticket.Booking.BusId);

        //    // Create the ViewModel
        //    var ticketDetailsViewModel = new TicketDetailsViewModel
        //    {
        //        TicketNumber = ticket.TicketNo,
        //        PNR = ticket.PNR,
        //        PassengerName = ticket.Booking.Passenger.Name,
        //        StartLocation = schedule?.Route?.StartLocation ?? "Unknown",
        //        EndLocation = schedule?.Route?.EndLocation ?? "Unknown",
        //        DepartureTime = schedule?.DepartureTime ?? DateTime.MinValue,
        //        ArrivalTime = schedule?.ArrivalTime ?? DateTime.MinValue,
        //        BusName = ticket.Booking.Bus.BusName,
        //        BusNumber = ticket.Booking.Bus.BusNumber,
        //        SeatNumbers = ticket.Booking.Bus.Seats.Select(s => s.SeatNumber).ToList(),
        //        TotalAmount = ticket.Price,
        //        PickupPoint = ticket.Booking.Passenger.BoardingPoint,
        //        DropPoint = ticket.Booking.Passenger.DroppingPoint

        //    };
        //    ViewBag.BookingId = bookingId;
        //    ViewBag.TicketId = ticket.TicketId;
        //    return View(ticketDetailsViewModel);
        //}

        [HttpGet]
        public async Task<IActionResult> TicketDetails(int bookingId)
        {
            // Fetch the ticket and related booking, bus, and passenger details
            var ticket = await _context.Ticket_Table
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Passenger)
                .Include(t => t.Booking.Bus)
                .FirstOrDefaultAsync(t => t.BookingId == bookingId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // Fetch the IoT device linked to the bus
            var iotDevice = await _context.IoTDevices
                .Where(d => d.BusId == ticket.Booking.BusId)
                .FirstOrDefaultAsync();

            if (iotDevice == null)
            {
                ViewBag.NoDeviceConnected = true;
            }
            else
            {
                ViewBag.NoDeviceConnected = false;
            }
            // Fetch Schedule details using BusId
            var schedule = await _context.Schedule_Table
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.BusId == ticket.Booking.BusId);

            var ticketDetailsViewModel = new TicketDetailsViewModel
            {
                TicketNumber = ticket.TicketNo,
                PNR = ticket.PNR,
                PassengerName = ticket.Booking.Passenger.Name,
                StartLocation = schedule?.Route?.StartLocation ?? "Unknown",
                EndLocation = schedule?.Route?.EndLocation ?? "Unknown",
                DepartureTime = schedule?.DepartureTime ?? DateTime.MinValue,
                ArrivalTime = schedule?.ArrivalTime ?? DateTime.MinValue,
                BusName = ticket.Booking.Bus.BusName,
                BusNumber = ticket.Booking.Bus.BusNumber,
                SeatNumbers = ticket.Booking.Bus.Seats.Select(s => s.SeatNumber).ToList(),
                TotalAmount = ticket.Price,
                PickupPoint = ticket.Booking.Passenger.BoardingPoint,
                DropPoint = ticket.Booking.Passenger.DroppingPoint,
                Latitude = iotDevice?.Latitude ?? (decimal)27.7614,
                Longitude = iotDevice?.Longitude ?? (decimal)85.3156,
                DeviceIdentifier = iotDevice?.DeviceIdentifier ?? "No Device",
                LastUpdated = iotDevice?.LastUpdated ?? DateTime.MinValue
            };
            ViewBag.BookingId = bookingId;
            ViewBag.TicketId = ticket.TicketId;

            return View(ticketDetailsViewModel);
        }


        public IActionResult Notification()
        {
            return View();
        }
        public IActionResult ErrorPage()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using YatriSewa.Models;
using YatriSewa.Services;

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



        [HttpPost]
        public async Task<IActionResult> SearchBuses(string from, string to, DateTime date)
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
        public IActionResult Payment(string token)
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



        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentConfirmationRequest request)
        {
            try
            {
                // Step 1: Validate input data
                if (request == null ||
                    string.IsNullOrEmpty(request.PhoneNumber) ||
                    string.IsNullOrEmpty(request.FullName) ||
                    request.SeatNumbers == null ||
                    !request.SeatNumbers.Any())
                {
                    return BadRequest("Invalid payment request data.");
                }

                // Step 2: Fetch or create Passenger entry
                var passenger = await _context.Passenger_Table
                    .FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber);

                if (passenger == null)
                {
                    passenger = new Passenger
                    {
                        Name = request.FullName,
                        PhoneNumber = request.PhoneNumber,
                        BoardingPoint = request.BoardingPoint,
                        DroppingPoint = request.DroppingPoint
                    };
                    _context.Passenger_Table.Add(passenger);
                    await _context.SaveChangesAsync();
                }

                // Step 3: Create a new Booking entry
                var booking = new Booking
                {
                    PassengerId = passenger.PassengerId,
                    BusId = request.BusId,
                    TotalSeats = request.SeatNumbers.Count,
                    TotalAmount = request.TotalAmount,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending // Mark as pending until payment is processed
                };
                _context.Booking_Table.Add(booking);
                await _context.SaveChangesAsync();

                // Step 4: Create Tickets and update Seat status
                foreach (var seatNumber in request.SeatNumbers)
                {
                    // Check if the seat exists in the Seat_Table
                    var seat = await _context.Seat_Table
                        .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber && s.BusId == request.BusId);

                    if (seat == null)
                    {
                        return BadRequest($"Seat {seatNumber} does not exist or is invalid.");
                    }

                    if (seat.Status != SeatStatus.Reserved)
                    {
                        return BadRequest($"Seat {seatNumber} is not reserved and cannot be booked.");
                    }

                    // Update seat status to Booked
                    seat.Status = SeatStatus.Booked;

                    // Create a ticket for the booking
                    var ticket = new Ticket
                    {
                        BookingId = booking.BookingId,
                        SeatId = seat.SeatId,
                        TicketNo = request.TicketNumber,
                        Price = request.PricePerSeat
                    };
                    _context.Ticket_Table.Add(ticket);
                }

                await _context.SaveChangesAsync();

                // Step 5: Redirect to payment card page based on payment method
                string redirectUrl = request.PaymentMethod switch
                {
                    "Esewa" => Url.Action("Esewa", "PaymentCard",
                        new { bookingId = booking.BookingId, totalAmount = request.TotalAmount }),
                    "Stripe" => Url.Action("Stripe", "PaymentCard",
                        new { bookingId = booking.BookingId, totalAmount = request.TotalAmount }),
                    _ => null
                };

                if (redirectUrl == null)
                {
                    return BadRequest("Unsupported payment method selected.");
                }

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmPayment: {ex.Message}");
                return BadRequest("An error occurred while processing your request.");
            }
        }




        public IActionResult PaymentCard()
        {
            return View();
        }
        public IActionResult Ticket()
        {
            return View();
        }
        public IActionResult Notification()
        {
            return View();
        }

    }
}

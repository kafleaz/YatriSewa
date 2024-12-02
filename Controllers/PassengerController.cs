using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YatriSewa.Models;

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
                return RedirectToAction("PassengerDashboard", "Passenger");
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
                return RedirectToAction("PassengerDashboard", "Passenger");
            }

            // If model state is not valid, return to the form with validation errors
            return View("OperatorRequest", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User updatedUser, IFormFile? profilePic)
        {
            var user = await _context.User_Table.FindAsync(updatedUser.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Update user details
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.PhoneNo = updatedUser.PhoneNo;

            // Handle profile picture update
            if (profilePic != null)
            {
                var filePath = Path.Combine("wwwroot/uploads", Path.GetFileName(profilePic.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePic.CopyToAsync(stream);
                }
                user.ProfilePicPath = "/uploads/" + Path.GetFileName(profilePic.FileName);
            }

            // Save changes
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile");
        }


        public IActionResult HomePage()
        {
            return View();
        }
        public IActionResult BusListing()
        {
            return View();
        }
        public IActionResult BusDetails()
        {
            return View();
        }
        public IActionResult SeatSelection()
        {
            return View();
        }
        public IActionResult Payment()
        {
            return View();
        }
        public IActionResult PaymentCard()
        {
            return View();
        }
        public IActionResult Ticket()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult Notification()
        {
            return View();
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using YatriSewa.Services;
using YatriSewa.Models;

namespace YatriSewa.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IEmailService _emailService;

        public LoginController(ApplicationContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Landing ()
        {
            return View();
        }
        public IActionResult GetStarted()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        // Helper method to validate email format
        private bool IsValidEmail(string input)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, emailPattern);
        }

        // Helper method to validate phone number format
        private bool IsValidPhone(string input)
        {
            var phonePattern = @"^9\d{9}$"; // Assuming 10-digit phone numbers starting with 9
            return Regex.IsMatch(input, phonePattern);
        }

        // Generate a 6-digit OTP
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();  // Generate a 6-digit OTP
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserRegistrationModel model)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                ViewBag.Errors = errors;
                return View("SignUp", model);  // Re-render the Signup page with validation errors
            }

            // Extract the EmailOrPhone field and initialize variables
            string? email = null;
            string? phoneNo = null;
            AuthMethod authMethod;

            // First, check if the input is a valid email or phone number
            if (IsValidEmail(model.PhoneEmail))
            {
                email = model.PhoneEmail;
                phoneNo = null;
                authMethod = AuthMethod.Email_password;
            }
            else if (IsValidPhone(model.PhoneEmail))
            {
                phoneNo = model.PhoneEmail;
                email = null;
                authMethod = AuthMethod.Phone_no;
            }
            else
            {
                // If neither email nor phone format is valid, display an error
                ModelState.AddModelError("PhoneEmail", "Please enter a valid email or phone number.");
                return View("SignUp", model);  // Re-render with error
            }

            // Ensure the email or phone number is unique
            if (!string.IsNullOrEmpty(email))
            {
                var existingUserByEmail = await _context.User_Table.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("PhoneEmail", "Email already registered. Please go back and Login");
                    return View("SignUp", model);  // Re-render with error
                }
            }

            if (!string.IsNullOrEmpty(phoneNo))
            {
                var existingUserByPhone = await _context.User_Table.FirstOrDefaultAsync(u => u.PhoneNo == phoneNo);
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError("PhoneEmail", "Phone no. already registered. Please go back and Login");
                    return View("SignUp", model);  // Re-render with error
                }
            }

            // Generate OTP
            var otp = int.Parse(GenerateOTP());

            // Create the new user object
            var newUser = new User
            {
                Name = string.IsNullOrEmpty(model.FullName) ? "Unknown" : model.FullName,
                Email = email,
                PhoneNo = phoneNo,
                Password = PasswordHashingService.HashPassword(model.Password),
                Role = UserRole.Passenger,
                Auth_Method = authMethod,
                IsVerified = false,
                OTP = otp,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };

            // Save the new user to the database
            _context.User_Table.Add(newUser);
            await _context.SaveChangesAsync();

            // Store email or phone number in session for OTP verification
            string emailOrPhone = email ?? phoneNo ?? string.Empty;  // Ensure it's not null
            HttpContext.Session.SetString("PhoneEmail", emailOrPhone);

            // Send OTP via email or phone (based on the input type)
            await _emailService.SendEmailAsync(emailOrPhone, "Registration OTP", $"Your OTP is {otp}");

            // Store the data in TempData for redirecting to OTP verification page
            TempData["EmailOrPhone"] = emailOrPhone;
            TempData["ShowOtpForm"] = "true";

            // Redirect to OTP form page
            return RedirectToAction("Verification");
        }

        public IActionResult Verification()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }
    }
}

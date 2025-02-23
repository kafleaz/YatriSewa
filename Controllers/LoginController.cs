using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using YatriSewa.Services;
using YatriSewa.Models;
using System;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace YatriSewa.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;

        public LoginController(ApplicationContext context, IEmailService emailService, ISMSService smsService)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
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

        // Generate a 6-digit OTP
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();  // Generate a 6-digit OTP
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserRegistrationModel model)
        {
            // Validate the model state using custom validation attributes
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

            // Since PhoneOrEmailAttribute ensures it's either a phone or email, just check which one
            if (model.PhoneEmail.Contains("@")) // Simple check for email
            {
                email = model.PhoneEmail;
                authMethod = AuthMethod.Email_password;
            }
            else
            {
                phoneNo = model.PhoneEmail;
                authMethod = AuthMethod.Phone_no;
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
            if (!string.IsNullOrEmpty(email))
            {
                // Send OTP via email
                await _emailService.SendEmailAsync(email, "Registration OTP", $"Your OTP is {otp}");
            }
            else if (!string.IsNullOrEmpty(phoneNo))
            {
                // Send OTP via SMS (you would need to implement the SMS sending functionality)
                await _smsService.SendSmsAsync(phoneNo, $"Your OTP is {otp}");  // Assuming you have an SMS service
            }

            // Store the data in TempData for redirecting to OTP verification page
            TempData["EmailOrPhone"] = emailOrPhone;
            TempData["ShowOtpForm"] = "true";

            // Redirect to OTP form page
            return RedirectToAction("Verification");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            // Combine the OTP input fields into a single string
            var enteredOtp = $"{otp1}{otp2}{otp3}{otp4}{otp5}{otp6}";

            // Ensure the OTP is not empty and consists only of numeric characters
            if (string.IsNullOrEmpty(enteredOtp) || !enteredOtp.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid 6-digit OTP.");
                TempData.Keep("EmailOrPhone"); // Keep TempData value for email/phone display
                return View("Verification");  // Return the same page with the error
            }

            // Retrieve email or phone from session or TempData
            var emailOrPhone = HttpContext.Session.GetString("PhoneEmail") ?? TempData["EmailOrPhone"]?.ToString();

            if (string.IsNullOrEmpty(emailOrPhone))
            {
                ModelState.AddModelError(string.Empty, "Session expired, please try again.");
                return RedirectToAction("SignUp");
            }

            // Try to parse the OTP
            if (!int.TryParse(enteredOtp, out var parsedOtp))
            {
                ModelState.AddModelError(string.Empty, "OTP format is invalid.");
                TempData.Keep("EmailOrPhone"); // Keep TempData value for email/phone display
                return View("Verification");
            }

            // Retrieve the user from the database based on email/phone and the OTP
            var user = await _context.User_Table
                .FirstOrDefaultAsync(u => (u.Email == emailOrPhone || u.PhoneNo == emailOrPhone) && u.OTP == parsedOtp);

            if (user == null)
            {
                // If OTP is incorrect, show error message
                ModelState.AddModelError(string.Empty, "Incorrect OTP. Please try again.");
                TempData.Keep("EmailOrPhone"); // Keep TempData value for email/phone display
                return View("Verification");  // Show error message on the same page
            }

            // If OTP is valid, mark the user as verified and clear the OTP
            user.IsVerified = true;
            user.OTP = 0; // Clear OTP after successful verification
            _context.User_Table.Update(user);
            await _context.SaveChangesAsync();

            // Redirect to the login page after successful verification
            return RedirectToAction("SignIn");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPasswordResetOTP(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            var enteredOtp = $"{otp1}{otp2}{otp3}{otp4}{otp5}{otp6}";

            if (string.IsNullOrEmpty(enteredOtp) || !enteredOtp.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid 6-digit OTP.");
                return View("ForgotPasswordVerification"); // Return the password reset OTP verification view
            }

            // Retrieve email or phone from session
            var emailOrPhone = HttpContext.Session.GetString("EmailOrPhone");
            if (string.IsNullOrEmpty(emailOrPhone))
            {
                ModelState.AddModelError(string.Empty, "Session expired, please try again.");
                return RedirectToAction("ForgotPassword");
            }

            // Validate the OTP
            if (!int.TryParse(enteredOtp, out var parsedOtp))
            {
                ModelState.AddModelError(string.Empty, "OTP format is invalid.");
                return View("ForgotPasswordVerification");
            }

            // Find the user by email/phone and OTP
            var user = await _context.User_Table
                .FirstOrDefaultAsync(u => (u.Email == emailOrPhone || u.PhoneNo == emailOrPhone) && u.OTP == parsedOtp);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Incorrect OTP. Please try again.");
                return View("ForgotPasswordVerification");
            }

            // Retrieve the new password from session
            var newPassword = HttpContext.Session.GetString("NewPassword");
            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please try again.");
                return RedirectToAction("ForgotPassword");
            }

            // Update the user's password
            user.Password = PasswordHashingService.HashPassword(newPassword);
            user.OTP = 0;  // Clear OTP after successful password reset
            user.Updated_At = DateTime.UtcNow;  // Update the timestamp
            _context.User_Table.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully. Please log in.";
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public async Task<IActionResult> ResendOTP()
        {
            // Retrieve email or phone from session
            var emailOrPhone = HttpContext.Session.GetString("PhoneEmail");

            if (string.IsNullOrEmpty(emailOrPhone))
            {
                // If no email or phone is in session, redirect to SignUp
                ModelState.AddModelError(string.Empty, "Session expired, please try again.");
                return RedirectToAction("SignUp");
            }

            // Generate a new OTP
            var newOtp = GenerateOTP();

            // Find the user associated with the email or phone
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.PhoneNo == emailOrPhone);

            if (user != null)
            {
                // Update the user's OTP
                user.OTP = int.Parse(newOtp);
                _context.User_Table.Update(user);
                await _context.SaveChangesAsync();

                // Send the OTP either via email or SMS
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendEmailAsync(user.Email, "Your OTP Code", $"Your new OTP is {newOtp}");
                }
                else if (!string.IsNullOrEmpty(user.PhoneNo))
                {
                    await _smsService.SendSmsAsync(user.PhoneNo, $"Your new OTP is {newOtp}");
                }

                TempData["EmailOrPhone"] = emailOrPhone;  // Keep the email/phone in TempData
                TempData["SuccessMessage"] = "A new OTP has been sent successfully!";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User not found. Please try again.");
                return RedirectToAction("SignUp");
            }

            return RedirectToAction("Verification");
        }


        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check whether the input is an email or phone number
            string? email = null;
            string? phoneNo = null;

            if (model.PhoneEmail.Contains("@"))  // Simple check for email
            {
                email = model.PhoneEmail;
            }
            else
            {
                phoneNo = model.PhoneEmail;
            }

            // Query the database to find the user by email or phone number
            User? user = null;
            if (!string.IsNullOrEmpty(email))
            {
                user = await _context.User_Table.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    ModelState.AddModelError("PhoneEmail", "Email is not registered. Please sign up.");
                    return View(model);
                }
            }
            else if (!string.IsNullOrEmpty(phoneNo))
            {
                user = await _context.User_Table.FirstOrDefaultAsync(u => u.PhoneNo == phoneNo);
                if (user == null)
                {
                    ModelState.AddModelError("PhoneEmail", "Phone number is not registered. Please sign up.");
                    return View(model);
                }
            }
            if (user == null)
            {
                ModelState.AddModelError("PhoneEmail", "User not found.");
                return View(model); // Return the view with an error
            }
            // Verify the password
            if (!PasswordHashingService.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("Password", "Incorrect password. Please try again.");
                return View(model);
            }

            // Check if the user is verified
            if (!user.IsVerified)
            {
                ModelState.AddModelError(string.Empty, "Your account is not verified. Please verify it before signing in.");
                return View(model);
            }

            // Create claims and set up the authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),  // Add UserId to claims
                new Claim(ClaimTypes.Name, user.Email ?? user.PhoneNo ?? "UnknownUser"),  // Use a fallback if both are null
                new Claim(ClaimTypes.Role, user.Role.ToString())  // This should be safe since user.Role is an enum
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Redirect based on ReturnUrl or Role
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Console.WriteLine($"Redirecting to ReturnUrl: {returnUrl}");
                return Redirect(returnUrl);
            }

            Console.WriteLine($"Redirecting based on role: {user.Role}");
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("AdminDashboard", "Admin"),
                UserRole.Operator => RedirectToAction("ListBus", "Operator"),
                UserRole.Passenger => RedirectToAction("HomePage", "Passenger"),
                UserRole.Driver => RedirectToAction("DriverDashboard", "Driver"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find the user by email or phone number
            var user = await _context.User_Table.FirstOrDefaultAsync(u => u.Email == model.EmailOrPhone || u.PhoneNo == model.EmailOrPhone);
            if (user == null)
            {
                ModelState.AddModelError("EmailOrPhone", "No user found with this email or phone number.");
                return View(model);
            }

            // Generate OTP
            var otp = GenerateOTP();

            // Save OTP and update Updated_At timestamp
            user.OTP = int.Parse(otp);
            user.Updated_At = DateTime.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Send OTP via email or phone
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(user.Email, "Password Reset OTP", $"Your OTP is: {otp}");
            }
            else if (!string.IsNullOrEmpty(user.PhoneNo))
            {
                await _smsService.SendSmsAsync(user.PhoneNo, $"Your password reset OTP is: {otp}");
            }

            // Store necessary data in session for OTP verification
            HttpContext.Session.SetString("EmailOrPhone", model.EmailOrPhone);
            HttpContext.Session.SetString("NewPassword", model.NewPassword);

            return RedirectToAction("ForgotPasswordVerification");  // Redirect to OTP verification page
        }

        //[HttpPost]
        [HttpPost, ActionName("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Sign out the user from authentication
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("SignIn", "Login"); // Redirect to login
        }

        public IActionResult ForgotPasswordVerification()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
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

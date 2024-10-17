using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace YatriSewa.Validators  // Change namespace to match your project structure
{
    public class PhoneOrEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Phone or Email is required.");
            }

            string input = value.ToString()!;
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";  // Basic email pattern
            var phonePattern = @"^9\d{9}$";  // Assuming phone numbers start with '9' and are 10 digits long

            // Check if the input matches either email or phone number pattern
            bool isEmail = Regex.IsMatch(input, emailPattern);
            bool isPhone = Regex.IsMatch(input, phonePattern);

            if (!isEmail && !isPhone)
            {
                return new ValidationResult("Please enter a valid email address or phone number starting with 9.");
            }

            return ValidationResult.Success!;
        }
    }
}

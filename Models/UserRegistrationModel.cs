using System.ComponentModel.DataAnnotations;
using YatriSewa.Validators;

namespace YatriSewa.Models
{
    public class UserRegistrationModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Phone or Email is required")]
        [PhoneOrEmail(ErrorMessage = "Please enter a valid email or a phone number")]
        public required string PhoneEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public required string Password { get; set; }
    }

    public class UserLoginModel
    {
        [Required(ErrorMessage = "Phone or Email is required")]
        public required string PhoneEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}

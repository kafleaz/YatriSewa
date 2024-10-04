using System.ComponentModel.DataAnnotations;

namespace YatriSewa.Models
{
    public class UserRegistrationModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Phone or Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public required string PhoneEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public required string Password { get; set; }
    }
}

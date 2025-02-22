using static System.Net.WebRequestMethods;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YatriSewa.Models
{
    public enum AuthMethod
    {
        Phone_no,
        Google,
        Email_password,
        // Add more methods as necessary
    }

    public enum UserRole
    {
        Admin,
        Passenger,
        Operator,
        Driver
        // Add more roles as necessary
    }
    public class RoleApprovalRequest
    {
        public int UserId { get; set; }
        public string Role { get; set; }
    }
    public class User
    {
        [Key]
        public int UserId { get; set; } // Primary key

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        //[Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; } = "DefaultPassword123";

        public string? PhoneNo { get; set; }

        [StringLength(255)]
        public string? Google_Id { get; set; }

        [Required]
        public AuthMethod Auth_Method { get; set; } // ENUM

        public bool IsVerified { get; set; } = false; // Boolean flag for verification

        [Required]
        public UserRole Role { get; set; } // ENUM for user roles

        [StringLength(255)]
        public string? ProfilePicPath { get; set; } // Store profile picture as a byte array

        public DateTime Created_At { get; set; } = DateTime.UtcNow; // Timestamp for record creation

        public DateTime Updated_At { get; set; } = DateTime.UtcNow; // Timestamp for last update
        public int OTP { get; set; }
        public ICollection<OTP> Otp_Table { get; set; } = new List<OTP>();// One-to-many relationship with OTPs

        public int? CompanyID { get; set; } // Foreign Key to BusCompany
        [ForeignKey("CompanyID")]
        public virtual BusCompany? BusCompany { get; set; }

        public int? DriverId { get; set; }
        public virtual BusDriver? BusDriver { get; set; }
        // Add this to associate the user with a bus
        public int? BusId { get; set; } // Nullable if the user is not yet assigned to a bus


        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>(); // One-to-many with bookings
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }


}

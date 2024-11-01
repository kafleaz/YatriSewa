using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YatriSewa.Models
{
    public class BusDriver
    {
        [Key]
        public int DriverId { get; set; }

        [Required]
        [StringLength(30)]
        public required string DriverName { get; set; }

        [Required]
        [StringLength(30)]
        public required string LicenseNumber { get; set; }

        [Required]
        [StringLength(40)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public required string Address { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }  // Removed nullable type since it's required

        [StringLength(255)]
        public string? LicensePhotoPath { get; set; }  // Can be optional for now

        public bool IsAvailable { get; set; } = true;  // Default to true
        public bool IsAssigned { get; set; } = false;  // Track assignment status

        // Link to the User entity
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Nullable link to BusCompany for company association
        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual BusCompany? BusCompany { get; set; }

        // One-to-Many: One driver can be assigned to multiple buses over time
        public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }

    public class DriverAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        public int? BusId { get; set; }
        [ForeignKey("BusId")]
        public virtual Bus? Bus { get; set; }

        public int? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual BusDriver? BusDriver { get; set; }

        [Required]
        public DateTime StartDate { get; set; }  // Assignment start date

        public DateTime? EndDate { get; set; }  // Assignment end date, if applicable
    }

}

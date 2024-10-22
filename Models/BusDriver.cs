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
        [StringLength(50)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public required string Address { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }  // Removed nullable type since it's required

        [StringLength(255)]
        public string? LicensePhotoPath { get; set; }  // Can be optional for now

        public bool IsAvailable { get; set; } = true;  // Default to true

        // Link to the User entity
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // One-to-Many: One driver can be assigned to multiple buses over time
        public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}

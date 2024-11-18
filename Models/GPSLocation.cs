using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YatriSewa.Models
{
    public class IoTDevice
    {
        [Key]
        public int DeviceId { get; set; } // Unique ID for each device record

        [MaxLength(50)]
        public string? SerialNumber { get; set; } // Unique serial number of the IoT device

        [ForeignKey("BusId")]
        public int? BusId { get; set; } // Associated BusId
        public virtual Bus? Bus { get; set; } // Navigation property to Bus
    }

    public class IoTDeviceLocationLog
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public virtual Bus? Bus { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 7)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 7)")]
        public decimal Longitude { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Speed { get; set; }

        [StringLength(100)]
        public string? LocationDescription { get; set; }

        public bool IsMoving { get; set; } = true;
        public bool HasPassengers { get; set; } = true;
    }

    public class PassengerLocationLog
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        public int PassengerId { get; set; }
        [ForeignKey("PassengerId")]
        public virtual Passenger? Passenger { get; set; }

        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public virtual Bus? Bus { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 7)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 7)")]
        public decimal Longitude { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(100)]
        public string? LocationDescription { get; set; }
    }


}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YatriSewa.Models
{
    public class BusCompany
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public required string CompanyName { get; set; }

        [StringLength(255)]
        public string? CompanyAddress { get; set; }

        [StringLength(50)]
        public string? ContactInfo { get; set; }

        [StringLength(50)]
        public string? Reg_No { get; set; }

        [StringLength(50)]
        public string? VAT_PAN { get; set; }

        public string? VAT_PAN_PhotoPath { get; set; }

        public int UserId { get; set; }


        // One-to-Many relationship: A company can have multiple buses
        public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }



    public class Bus
    {
        [Key]
        public int BusId { get; set; }

        [Required]
        [StringLength(50)]
        public required string BusName { get; set; }

        [Required]
        [StringLength(50)]
        public required string BusNumber { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }
        public int SeatCapacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // One-to-One relationship with Service
        public virtual Service? Service { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; } = [];
        public virtual ICollection<Seat> Seats { get; set; } = [];
        public virtual ICollection<Ticket> Tickets { get; set; } = [];

        // Foreign Key to BusCompany (One BusCompany manages multiple buses)
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual BusCompany? BusCompany { get; set; }

        // Foreign Key to Route (Each bus operates on one route at a time)
        public int? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual Route? Route { get; set; }  // A bus operates on one route

        public int? DriverId { get; set; }
        public virtual BusDriver? BusDriver { get; set; }
    }



    public class Route
    {
        [Key]
        public int RouteID { get; set; }

        [Required]
        [StringLength(255)]
        public required string StartLocation { get; set; }

        [StringLength(255)]
        public string? Stops { get; set; }

        [Required]
        [StringLength(255)]
        public required string EndLocation { get; set; }

        public string? EstimatedTime { get; set; }
        // New properties for geolocation
        public decimal? StartLatitude { get; set; }
        public decimal? StartLongitude { get; set; }
        public decimal? EndLatitude { get; set; }
        public decimal? EndLongitude { get; set; }
        // Foreign Key to BusCompany (Route belongs to a bus company)
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public BusCompany? BusCompany { get; set; }

        // One-to-Many relationship: A route can have multiple buses
        public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }

    public enum BusType
    {
        Seater,
        Sleeper,
        AirBus
    }
    public enum SeatType
    {
        ixii,
        iixii,
    }

    public class Service
    {
        [Key]
        public int ServiceId { get; set; }  // Primary key for the Service table

        // Wi-Fi availability (true/false)
        [Required(ErrorMessage = "Wifi status is required")]
        public bool Wifi { get; set; }

        // AC availability (true/false)
        [Required(ErrorMessage = "AC status is required")]
        public bool AC { get; set; }

        public BusType? BusType { get; set; }

        public SeatType? SeatType { get; set; }
        // Safety features, such as sanitization or masks
        [StringLength(255, ErrorMessage = "Safety Features cannot exceed 255 characters")]
        [Column("safety_features")]
        public string? SafetyFeatures { get; set; }

        // Essentials provided (e.g., pillows, water)
        [StringLength(255, ErrorMessage = "Essentials cannot exceed 255 characters")]
        [Column("essentials")]
        public string? Essentials { get; set; }

        // Snacks available on the bus
        [StringLength(255, ErrorMessage = "Snacks cannot exceed 255 characters")]
        [Column("snacks")]
        public string? Snacks { get; set; }

        [ForeignKey("Bus")]
        public int BusId { get; set; }

        public virtual Bus? Bus { get; set; }
    }

    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }  // Primary Key

        [ForeignKey("BusId")]
        public int? BusId { get; set; }  // Foreign Key to Bus Table
        public virtual Bus? Bus { get; set; }  // Navigation property

        [ForeignKey("RouteId")]
        public int RouteId { get; set; }  // Foreign Key to Route Table
        public virtual Route? Route { get; set; }  // Navigation property

        public DateTime DepartureTime { get; set; }  // Departure time of the schedule
        public DateTime ArrivalTime { get; set; }  // Arrival time of the schedule

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }  // Ticket price

        public int? AvailableSeats { get; set; }  // Number of available seats

        [StringLength(50)]
        public string? Status { get; set; }  // Status of the schedule

        public int? DriverId { get; set; }  // Foreign Key to Driver Table
        [ForeignKey("DriverId")]
        public virtual BusDriver? Driver { get; set; }  // Navigation property

        public int? BusCompanyId { get; set; }  // Foreign Key to BusCompany Table
        [ForeignKey("BusCompanyId")]
        public virtual BusCompany? BusCompany { get; set; }  // Navigation property
    }



}

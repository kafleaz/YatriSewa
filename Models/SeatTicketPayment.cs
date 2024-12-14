using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace YatriSewa.Models
{
    public enum SeatStatus
    {
        Available,   // Seat is available for booking
        Reserved,    // Temporarily held for a user
        Booked,       // Seat is paid for and booked
        Unavailable // Seat is permanently unavailable
    }
    public enum BookingStatus
    {
        Pending,    // Booking is reserved but not paid
        Paid,       // Booking has been paid
        Cancelled   // Booking has been canceled
    }
    public enum PaymentMethod
    {
        Esewa,
        Khalti,
        Stripe
    }
    public enum PaymentStatus
    {
        Pending,     // Payment is in process
        Successful,  // Payment completed successfully
        Failed       // Payment failed
    }

    public class Seat
    {
        [Key]
        public int SeatId { get; set; } // Primary key

        [Required]
        [StringLength(10)]
        public string SeatNumber { get; set; } = string.Empty; // A1, A2, etc.

        [Required]
        public SeatStatus Status { get; set; } = SeatStatus.Available; // Enum for Unavailable, Available, Reserved, Booked

        [ForeignKey("Bus")]
        public int BusId { get; set; } // Foreign key to the bus
        public virtual Bus? Bus { get; set; } // Navigation property

        [ForeignKey("Booking")]
        public int? BookingId { get; set; } // Foreign key to the booking
        public virtual Booking? Booking { get; set; } // Navigation property

        public int? ReservedByUserId { get; set; } // User who reserved the seat
        [ForeignKey("ReservedByUserId")]
        public virtual User? ReservedByUser { get; set; } // Navigation property

        public DateTime? ReservedAt { get; set; } // Timestamp for when the seat was reserved
    }


    public class Booking
    {
        [Key]
        public int BookingId { get; set; } // Primary key

        [ForeignKey("User")]
        public int UserId { get; set; } // Foreign key to the user
        public virtual User? User { get; set; } // Navigation property

        [ForeignKey("Bus")]
        public int? BusId { get; set; } // Foreign key to the bus

        [ForeignKey("Passenger")]
        public int? PassengerId { get; set; }
        public virtual Bus? Bus { get; set; } // Navigation property

        [Required]
        public int? TotalSeats { get; set; } // Total seats booked

        [Required]
        public DateTime BookingDate { get; set; } // Booking date and time

        [Required]
        [StringLength(20)]
        public BookingStatus Status { get; set; } // Pending, Paid, Cancelled

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; } // Total amount for the booking
        public virtual Passenger? Passenger { get; set; }
        public virtual ICollection<Ticket>? Tickets { get; set; } // Navigation property
    }

    public class Ticket
    {
        [Key]
        public int TicketId { get; set; } // Primary key

        [ForeignKey("Booking")]
        public int BookingId { get; set; } // Foreign key to the booking
        public virtual Booking? Booking { get; set; } // Navigation property

        [ForeignKey("Seat")]
        public int SeatId { get; set; } // Foreign key to the seat
        public virtual Seat? Seat { get; set; } // Navigation property

        [StringLength(20)]
        public string TicketNo { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper(); // Auto-generated unique ticket number

        [StringLength(20)]
        public string PNR { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(); // Auto-generated unique PNR
        public virtual Passenger? Passenger { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; } // Ticket price
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; } // Primary key

        [ForeignKey("Booking")]
        public int BookingId { get; set; } // Foreign key to the booking
        public virtual Booking? Booking { get; set; } // Navigation property

        [Required]
        public DateTime PaymentDate { get; set; } // Payment date and time

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal AmountPaid { get; set; } // Amount paid

        [Required]
        [StringLength(20)]
        public  PaymentMethod PaymentMethod { get; set; } // Card, Cash, UPI, etc.

        [ForeignKey("Passenger")]
        public int? PassengerId { get; set; } 
        public virtual Passenger? Passenger { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        [StringLength(20)]
        public PaymentStatus Status { get; set; } // Successful, Failed

        [ForeignKey("EsewaTransaction")]
        public int? TransactionId { get; set; } // Foreign key to EsewaTransaction
        public virtual EsewaTransaction? EsewaTransaction { get; set; }
    }


}

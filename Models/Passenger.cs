using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YatriSewa.Models
{
    public class Passenger
    {
        [Key]
        public int PassengerId { get; set; } // Primary key

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Passenger's name

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty; // Passenger's phone number

        [StringLength(50)]
        public string BoardingPoint { get; set; } = string.Empty; // Passenger's boarding point

        [StringLength(50)]
        public string DroppingPoint { get; set; } = string.Empty;
        public virtual ICollection<Booking>? Bookings { get; set; }
        public int BusId { get; set; }  // Foreign Key to Bus Table
        [ForeignKey("BusId")]
        public virtual Bus Bus { get; set; }
    }

    public class BusDetailsViewModel
    {
        public int BusId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime JourneyDate { get; set; }
        public string? BusName { get; set; }
        public string? ImagePath { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public decimal Price { get; set; }
        public bool Wifi { get; set; }
        public bool AC { get; set; }
        public DateTime? DepartureTime { get; set; }  // Departure time of the schedule
        public DateTime? ArrivalTime { get; set; }
        public string? SafetyFeatures { get; set; }
        public string? Description { get; set; }
        public string? Essentials { get; set; }
        public string? Snacks { get; set; }
        public SeatType? SeatType { get; set; }
        public int SeatCapacity { get; set; }
        public string? BusNumber { get; set; }
        public List<string> ReservedSeats { get; set; } = new List<string>();
        public List<string>? Reviews { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();

        public int TotalRows { get; set; } // Total full rows
        public int LastRowSeats { get; set; } // Seats in the last row
        public int SeatsPerRow => (SeatType ?? Models.SeatType.iixii) == Models.SeatType.iixii ? 4 : 3;



    }
    public class BookingViewModel
    {
        public int BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public decimal Price { get; set; }
        public string SelectedSeats { get; set; } = string.Empty; // Comma-separated seat numbers
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime JourneyDate { get; set; }
        public int? BookingId { get; set; } // Optional Booking ID
    }
    public class SeatSelectionViewModel
    {
        public int BusId { get; set; }
        public string? BusName { get; set; }
        public decimal Price { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime JourneyDate { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();
    }

    // Request DTO for deserialization
    public class ReserveSeatsRequest
    {
        public int BusId { get; set; }
        public List<string> SeatNumbers { get; set; } = new List<string>();
        public int ScheduleId { get; set; }

    }
    //public class BuyReservedSeatsRequest
    //{
    //    public int BusId { get; set; } // The ID of the bus for which the seats are being bought
    //    public List<string> SeatNumbers { get; set; } // The reserved seat numbers to buy
    //}
    public class PaymentViewModel
    {
        public int? BusId { get; set; }
        public string? BusName { get; set; }
        public int? PassengerId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TicketNumber { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public string? PickupPoint { get; set; }
        public string? PickupTime { get; set; }
        public string? DropPoint { get; set; }
        public string? DropTime { get; set; }
        public List<int>? ReservedSeatIds { get; set; }
        public string? TokenType { get; set; }
        public string BoardingPoint { get; set; } = string.Empty;
        public string DropingPoint { get; set; } = string.Empty;
    }

    public class PaymentConfirmationRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int BusId { get; set; }
        public required string SeatNumbers { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PricePerSeat { get; set; }
        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }
        public string? TicketNumber { get; set; }
        public string? BookingStatus { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class TicketDetailsViewModel
    {
        public string? TicketNumber { get; set; }
        public string? PNR { get; set; }
        public int PassengerId { get; set; } 
        public string? PassengerName { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public List<string>? SeatNumbers { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PickupPoint { get; set; }
        public string? DropPoint { get; set; }
        public string? DeviceIdentifier { get; set; } // Unique identifier, e.g., ESP8266-12345
        public decimal? Latitude { get; set; } // Latitude of the device
        public decimal? Longitude { get; set; } // Longitude of the device
        public decimal? Speed { get; set; } // Speed of the device
        public DateTime? LastUpdated { get; set; } // Timestamp of the last update
    }


}

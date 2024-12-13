namespace YatriSewa.Models
{
    public class TokenData
    {
        public string? TokenType { get; set; }
        public int ScheduleId { get; set; }
        public int BusId { get; set; }
        public List<string>? SeatNumbers { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime JourneyDate { get; set; }
        public int UserId { get; set; }
        public List<int>? ReservedSeatIds { get; set; }
    }

}

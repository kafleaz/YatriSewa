namespace YatriSewa.Models
{
    public class OTP
    {
        public int OtpId { get; set; } // Primary key for the OTP table

        public int OtpCode { get; set; } // The actual OTP code
        public DateTime ExpiresAt { get; set; } // The expiration time for the OTP

        public int UserId { get; set; } // Foreign key to the User table
        public required User User_Table { get; set; } // Navigation property for the User

        public bool IsUsed { get; set; } = false; // Flag to check if OTP has been used
    }
}

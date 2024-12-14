using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YatriSewa.Models
{
    public class EsewaTransaction
    {
        [Key]
        public int TransactionId { get; set; } // Primary key

        [Required]
        public string? MerchantCode { get; set; } // eSewa Merchant Code

        [StringLength(50)]
        public string? PaymentId { get; set; } // Unique Payment Identifier (pid)

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; } // Total Amount (tAmt)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxAmount { get; set; } = 0; // Tax Amount (txAmt)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ServiceCharge { get; set; } = 0; // Service Charge (psc)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ProductCharge { get; set; } = 0; // Product Charge (pcd)

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaidAmount { get; set; } // Paid Amount (amt)

        [StringLength(50)]
        public string ReferenceId { get; set; } = string.Empty; // eSewa Reference ID (refId)

        [StringLength(50)]
        public string BookingId { get; set; } = string.Empty; // Associated Booking ID

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Transaction Status: Pending, Success, Failed

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // Date and Time of Transaction
    }
    public class Merchant
    {
        [Key]
        public int MerchantId { get; set; } // Primary key

        [ForeignKey("CompanyId")]
        public int? CompanyId { get; set; } // Associated Company
        public virtual BusCompany? Company { get; set; } // Navigation property to BusCompany

        [ForeignKey("Driver")]
        public int? DriverId { get; set; } // Associated Driver
        public virtual BusDriver? Driver { get; set; } // Navigation property to BusDriver

        [Required]
        [StringLength(50)]
        public string MerchantCode { get; set; } = string.Empty; // eSewa Merchant Code

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxAmount { get; set; } = 0; // Tax Amount (txAmt)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ServiceCharge { get; set; } = 0; // Service Charge (psc)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ProductCharge { get; set; } = 0; // Product Charge (pdc)

        public bool IsActive { get; set; } = true; // Whether the merchant is currently active
    }

}

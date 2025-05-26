using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public enum CommissionStatus
    {
        Pending,
        Approved,
        Paid,
        Cancelled
    }

    public class Commission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MarketerId { get; set; }
        public Marketer Marketer { get; set; }

        [Required]
        public int ReferralId { get; set; }
        public Referral Referral { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }

        [Required]
        public CommissionStatus Status { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string PaymentReference { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
} 
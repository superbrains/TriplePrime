using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public enum ReferralStatus
    {
        Pending,
        Active,
        Completed,
        Expired,
        Cancelled
    }

    public class Referral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MarketerId { get; set; }

        [Required]
        public string ReferredUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferralCode { get; set; }

        public ReferralStatus Status { get; set; }

        public decimal CommissionAmount { get; set; }

        public bool CommissionPaid { get; set; }

        public DateTime ReferralDate { get; set; }

        public DateTime? CommissionPaidDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Marketer Marketer { get; set; }

        [ForeignKey("ReferredUserId")]
        public virtual ApplicationUser ReferredUser { get; set; }
    }
} 
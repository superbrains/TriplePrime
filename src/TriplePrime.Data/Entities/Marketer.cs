using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public class Marketer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(100)]
        public string Website { get; set; }

        [MaxLength(100)]
        public string SocialMediaHandle { get; set; }

        [Required]
        [Range(0, 0.25)]
        public decimal CommissionRate { get; set; }

        public decimal TotalCommissionEarned { get; set; }

        public decimal CurrentBalance { get; set; }

        public int TotalCustomers { get; set; }

        public decimal TotalSales { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; } = "Admin";

        // Navigation properties
        public ICollection<Referral> Referrals { get; set; }
        public ICollection<Commission> Commissions { get; set; }
    }
} 
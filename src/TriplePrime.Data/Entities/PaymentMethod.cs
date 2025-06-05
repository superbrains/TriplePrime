using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Entities
{
    public enum PaymentMethodType
    {
        CreditCard,
        DebitCard,
        BankTransfer,
        MobileMoney,
        Cash
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Provider { get; set; } // e.g., "Paystack"
        public string Type { get; set; } // e.g., "card", "bank", "mobile"
        public string LastFourDigits { get; set; }
        public string AuthorizationCode { get; set; } // Paystack authorization code
        public string CardType { get; set; }
        public string Bank { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
} 
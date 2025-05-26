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
        public PaymentMethodType Type { get; set; }
        public string Provider { get; set; } // Stripe, PayPal, etc.
        public string LastFourDigits { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
} 
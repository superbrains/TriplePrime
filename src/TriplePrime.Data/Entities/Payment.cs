using System;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Entities
{
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }

    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PaymentMethodId { get; set; }
        public int? FoodPackId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public string CallbackUrl { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual FoodPack FoodPack { get; set; }
    }
} 
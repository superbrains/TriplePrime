using System;
using System.Collections.Generic;

namespace TriplePrime.Data.Entities
{
    public class SavingsPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FoodPackId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string Status { get; set; } // Active, Completed, Cancelled
        public string PaymentPreference { get; set; } // Automatic, Manual
        public string PaymentFrequency { get; set; } // daily, weekly, monthly
        public int? PaymentMethodId { get; set; } // Reference to PaymentMethod entity
        public string PlanCode { get; set; } = "NA"; // Paystack plan code (nullable)=
        public string SubscriptionCode { get; set; } = "NA"; // Paystack subscription code (nullable)
        public bool RemindersEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = "Admin";
        public string UpdatedBy { get; set; } = "Admin";

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual FoodPack FoodPack { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual ICollection<PaymentSchedule> PaymentSchedules { get; set; }
    }
} 
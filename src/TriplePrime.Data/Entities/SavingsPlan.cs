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
        public decimal AmountPaid { get; set; }
        public decimal MonthlyPayment { get; set; }
        public int DurationInMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
        public string Status { get; set; } // Active, Completed, Cancelled
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual FoodPack FoodPack { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
} 
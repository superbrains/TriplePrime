using System;

namespace TriplePrime.Data.Entities
{
    public class PaymentSchedule
    {
        public int Id { get; set; }
        public int SavingsPlanId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // Pending, Paid, Overdue
        public string PaymentReference { get; set; } // Paystack reference
        public DateTime? PaidAt { get; set; }

        // Interest accrual fields
        public decimal AccruedInterest { get; set; } = 0m;
        public DateTime? InterestAccrualStartDate { get; set; }
        public DateTime? LastInterestCalculationDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual SavingsPlan SavingsPlan { get; set; }
    }
} 
using System;
using System.Collections.Generic;

namespace TriplePrime.Data.Models
{
    public class PaymentConfirmationMessage
    {
        public int PlanId { get; set; }
        public string UserId { get; set; }
        public int FoodPackId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentReference { get; set; }
        public decimal TotalAmount { get; set; }
        public int Duration { get; set; }
        public string PaymentFrequency { get; set; }
        public string PaymentPreference { get; set; }
        public IEnumerable<PaymentScheduleInfo> PaymentSchedules { get; set; }
    }

    public class PaymentScheduleInfo
    {
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
} 
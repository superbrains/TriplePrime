using System;
using System.Collections.Generic;

namespace TriplePrime.Data.Models
{
    public class SavingsPlanWithUserDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserPhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string UserAddress { get; set; }
        public string FoodPackName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string PaymentFrequency { get; set; }
        public List<PaymentScheduleDto> PaymentSchedules { get; set; }
    }

    public class PaymentScheduleDto
    {
        public int Id { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }
} 
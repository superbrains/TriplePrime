using System;

namespace TriplePrime.Data.Models
{
    public class RecentPaymentDto
    {
        public int PlanId { get; set; }
        public int ScheduleId { get; set; }
        public string UserFullName { get; set; }
        public string FoodPackName { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}

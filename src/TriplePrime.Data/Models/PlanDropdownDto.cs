using System.Collections.Generic;

namespace TriplePrime.Data.Models
{
    public class PlanDropdownDto
    {
        public int Id { get; set; }
        public string UserFullName { get; set; }
        public string FoodPackName { get; set; }
        public List<PaymentScheduleDto> PendingSchedules { get; set; }
    }
}

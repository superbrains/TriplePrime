using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Models
{
    public class SetPenaltyInterestDto
    {
        [Required(ErrorMessage = "Daily rate percentage is required")]
        [Range(0.1, 5.0, ErrorMessage = "Daily rate must be between 0.1% and 5%")]
        public decimal DailyRatePercent { get; set; }
    }
}

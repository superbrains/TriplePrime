using System;

namespace TriplePrime.Data.Entities
{
    public class FoodPackPricing
    {
        public int Id { get; set; }
        public int FoodPackId { get; set; }
        public int DurationMonths { get; set; }
        public decimal InterestRate { get; set; } // Stored as decimal (e.g., 0.02 for 2%, 0.04 for 4%)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual FoodPack FoodPack { get; set; }

        // Calculated property - not stored in DB
        public decimal GetTotalPrice(decimal basePrice)
        {
            return basePrice * (1 + InterestRate);
        }

        // Calculate daily payment amount
        public decimal GetDailyPaymentAmount(decimal basePrice)
        {
            var totalPrice = GetTotalPrice(basePrice);
            var totalDays = DurationMonths * 30;
            return totalPrice / totalDays;
        }
    }
}

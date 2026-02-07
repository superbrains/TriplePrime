using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Models
{
    public class FoodPackPricingDto
    {
        public int Id { get; set; }
        public int FoodPackId { get; set; }
        public int DurationMonths { get; set; }
        public decimal InterestRate { get; set; }
        public bool UseGlobalRate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DailyPaymentAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateFoodPackPricingDto
    {
        [Required]
        public int FoodPackId { get; set; }

        [Required]
        [Range(1, 4, ErrorMessage = "Duration must be between 1 and 4 months")]
        public int DurationMonths { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Interest rate must be between 0 and 1 (0% - 100%)")]
        public decimal InterestRate { get; set; }
    }

    public class UpdateFoodPackPricingDto
    {
        [Required]
        [Range(0, 1, ErrorMessage = "Interest rate must be between 0 and 1 (0% - 100%)")]
        public decimal InterestRate { get; set; }

        /// <summary>
        /// When true, this food pack uses the global pricing rate for this duration.
        /// When false, uses the InterestRate stored in this record (override).
        /// </summary>
        public bool UseGlobalRate { get; set; } = false;
    }

    public class FoodPackWithPricingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal Savings { get; set; }
        public bool Available { get; set; }
        public bool Featured { get; set; }
        public string ImageUrl { get; set; }
        public int Inventory { get; set; }
        public string Category { get; set; }
        public List<FoodPackPricingDto> Pricings { get; set; } = new List<FoodPackPricingDto>();
    }

    public class BulkCreateFoodPackPricingDto
    {
        [Required]
        public int FoodPackId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one pricing tier is required")]
        [MaxLength(4, ErrorMessage = "Maximum of 4 pricing tiers allowed (1-4 months)")]
        public List<PricingTier> PricingTiers { get; set; }
    }

    public class PricingTier
    {
        [Required]
        [Range(1, 4, ErrorMessage = "Duration must be between 1 and 4 months")]
        public int DurationMonths { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Interest rate must be between 0 and 1 (0% - 100%)")]
        public decimal InterestRate { get; set; }

        /// <summary>
        /// When true, this food pack uses the global pricing rate for this duration.
        /// When false, uses the InterestRate stored in this record (override).
        /// Defaults to true for new pricing tiers.
        /// </summary>
        public bool UseGlobalRate { get; set; } = true;
    }

    /// <summary>
    /// DTO for setting global pricing tiers
    /// </summary>
    public class GlobalPricingTiersDto
    {
        /// <summary>
        /// Dictionary with duration (1-4) as key and interest rate as value
        /// </summary>
        [Required]
        public Dictionary<int, decimal> Rates { get; set; }
    }

    /// <summary>
    /// DTO for toggling between global rate and override
    /// </summary>
    public class SetUseGlobalRateDto
    {
        /// <summary>
        /// When true, use the global rate. When false, use the override rate.
        /// </summary>
        [Required]
        public bool UseGlobal { get; set; }

        /// <summary>
        /// The override interest rate (only used when UseGlobal is false)
        /// </summary>
        [Range(0, 1, ErrorMessage = "Interest rate must be between 0 and 1 (0% - 100%)")]
        public decimal? OverrideRate { get; set; }
    }
}

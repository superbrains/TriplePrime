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
    }
}

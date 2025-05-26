using System;
using System.Collections.Generic;

namespace TriplePrime.Data.Entities
{
    public enum FoodPackStatus
    {
        Pending,
        Active,
        Inactive,
        OutOfStock,
        Discontinued
    }

    public class FoodPack
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public FoodPackStatus Status { get; set; }
        public decimal Rating { get; set; }
        public int PopularityScore { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Navigation Properties
        public virtual ICollection<FoodPackItem> Items { get; set; } = new List<FoodPackItem>();
        public virtual SavingsPlan SavingsPlan { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
} 
using System;

namespace TriplePrime.Data.Entities
{
    public class FoodPackItem
    {
        public int Id { get; set; }
        public int FoodPackId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } // kg, g, pieces, etc.
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual FoodPack FoodPack { get; set; }
    }
} 
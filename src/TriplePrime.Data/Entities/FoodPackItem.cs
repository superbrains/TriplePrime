using System;

namespace TriplePrime.Data.Entities
{
    public class FoodPackItem
    {
        public int Id { get; set; }
        public int FoodPackId { get; set; }
        public string Item { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual FoodPack FoodPack { get; set; }
    }
} 
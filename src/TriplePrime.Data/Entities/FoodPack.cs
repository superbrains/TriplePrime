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
        public decimal OriginalPrice { get; set; }
        public decimal Savings { get; set; }
        public bool Available { get; set; }
        public bool Featured { get; set; }
        public string ImageUrl { get; set; }
        public int Inventory { get; set; }
        public int Duration { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; } = "Admin";

        // Navigation Properties
        public virtual ICollection<FoodPackItem> Items { get; set; } = new List<FoodPackItem>();
    }
} 
using System;

namespace TriplePrime.Data.Entities
{
    public class FoodPackPurchase
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FoodPackId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Completed", "Cancelled"
        public DateTime? DeliveryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual FoodPack FoodPack { get; set; }
    }
} 
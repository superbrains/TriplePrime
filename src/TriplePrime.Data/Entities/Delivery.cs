using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public enum DeliveryStatus
    {
        Pending,
        Scheduled,
        InTransit,
        Delivered,
        Cancelled,
        Failed
    }

    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string DriverId { get; set; }
        public ApplicationUser Driver { get; set; }

        [Required]
        public int DeliveryAddressId { get; set; }
        public DeliveryAddress DeliveryAddress { get; set; }

        [Required]
        public int FoodPackId { get; set; }
        public FoodPack FoodPack { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        public DeliveryStatus Status { get; set; }

        public string TrackingNumber { get; set; }

        public string Notes { get; set; }

        public decimal DeliveryFee { get; set; }

        public bool IsPriority { get; set; }

        public string DeliveryInstructions { get; set; }

        public string CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
} 
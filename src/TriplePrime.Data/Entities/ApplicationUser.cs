using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Device token for push notifications
        [MaxLength(500)]
        public string DeviceToken { get; set; }

        // Preferences - Made nullable
        public NotificationPreferences NotificationPreferences { get; set; }
        public DeliveryPreferences DeliveryPreferences { get; set; }
        public string? LanguagePreference { get; set; }

        // Navigation properties - Made nullable
        public ICollection<DeliveryAddress> DeliveryAddresses { get; set; }
        public ICollection<PaymentMethod> PaymentMethods { get; set; }
        public ICollection<FoodPack> FoodPacks { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Referral> Referrals { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Delivery> Deliveries { get; set; }
        public ICollection<IdentityUserRole<string>> UserRoles { get; set; }
        public ICollection<SavingsPlan> SavingsPlans { get; set; }
    }

    public class NotificationPreferences
    {
        public int Id { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
    }

    public class DeliveryPreferences
    {
        public int Id { get; set; }
        public string PreferredDeliveryTime { get; set; }
        public string PreferredDeliveryDay { get; set; }
        public bool AllowContactlessDelivery { get; set; } = true;
    }
} 
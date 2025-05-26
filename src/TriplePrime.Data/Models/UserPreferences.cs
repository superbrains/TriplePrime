using System;

namespace TriplePrime.Data.Models
{
    public class UserPreferences
    {
        public UserNotificationPreferences NotificationPreferences { get; set; }
        public UserDeliveryPreferences DeliveryPreferences { get; set; }
        public string LanguagePreference { get; set; }
    }

    public class UserNotificationPreferences
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
    }

    public class UserDeliveryPreferences
    {
        public string PreferredDeliveryTime { get; set; }
        public string PreferredDeliveryDay { get; set; }
        public bool AllowContactlessDelivery { get; set; }
    }
    
    public class UserProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
} 
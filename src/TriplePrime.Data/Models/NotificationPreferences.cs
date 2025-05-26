namespace TriplePrime.Data.Models
{
    public class NotificationPreferences
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public string Theme { get; set; }
    }
} 
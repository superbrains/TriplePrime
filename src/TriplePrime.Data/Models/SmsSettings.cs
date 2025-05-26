namespace TriplePrime.Data.Models
{
    public class SmsSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromNumber { get; set; }
        public bool EnableDeliveryReports { get; set; }
        public int RetryAttempts { get; set; }
    }
} 
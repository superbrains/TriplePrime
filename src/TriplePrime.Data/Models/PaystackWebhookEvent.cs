using System;

namespace TriplePrime.Data.Models
{
    public class PaystackWebhookEvent
    {
        public string Event { get; set; }
        public PaystackVerifyData Data { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TriplePrime.Data.Models
{
    public class PaystackInitializeRequest
    {
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string CallbackUrl { get; set; }
        public string[] Channels { get; set; }
        public PaystackMetadata Metadata { get; set; }
    }

    public class PaystackMetadata
    {
        [JsonPropertyName("custom_fields")]
        public List<PaystackCustomField> CustomFields { get; set; }
        
        [JsonPropertyName("referrer")]
        public string Referrer { get; set; }
    }

    public class PaystackCustomField
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        
        [JsonPropertyName("variable_name")]
        public string VariableName { get; set; }
        
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }

    public class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackInitializeData Data { get; set; }
    }

    public class PaystackInitializeData
    {
        public string AuthorizationUrl { get; set; }
        public string AccessCode { get; set; }
        public string Reference { get; set; }
    }

    public class PaystackVerifyResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        [JsonPropertyName("data")]
        public PaystackVerifyData Data { get; set; }
    }

    public class PaystackVerifyData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonPropertyName("reference")]
        public string Reference { get; set; }
        
        [JsonPropertyName("receipt_number")]
        public object ReceiptNumber { get; set; }
        
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        
        [JsonPropertyName("message")]
        public object Message { get; set; }
        
        [JsonPropertyName("gateway_response")]
        public string GatewayResponse { get; set; }
        
        [JsonPropertyName("paid_at")]
        public DateTime PaidAt { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("channel")]
        public string Channel { get; set; }
        
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        
        [JsonPropertyName("ip_address")]
        public string IpAddress { get; set; }
        
        [JsonPropertyName("metadata")]
        public PaystackMetadata Metadata { get; set; }
        
        [JsonPropertyName("log")]
        public PaystackLog Log { get; set; }
        
        [JsonPropertyName("fees")]
        public int Fees { get; set; }
        
        [JsonPropertyName("fees_split")]
        public object FeesSplit { get; set; }
        
        [JsonPropertyName("authorization")]
        public PaystackAuthorization Authorization { get; set; }
        
        [JsonPropertyName("customer")]
        public PaystackCustomer Customer { get; set; }
        
        [JsonPropertyName("plan")]
        public object Plan { get; set; }
        
        [JsonPropertyName("requested_amount")]
        public int RequestedAmount { get; set; }
    }

    public class PaystackLog
    {
        public long StartTime { get; set; }
        public int TimeSpent { get; set; }
        public int Attempts { get; set; }
        public int Errors { get; set; }
        public bool Success { get; set; }
        public bool Mobile { get; set; }
        public List<object> Input { get; set; }
        public List<PaystackLogHistory> History { get; set; }
    }

    public class PaystackLogHistory
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public int Time { get; set; }
    }

    public class PaystackFee
    {
        public int Amount { get; set; }
        public object Formula { get; set; }
        public string Type { get; set; }
    }

    public class PaystackCustomer
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("customer_code")]
        public string CustomerCode { get; set; }
        
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        
        [JsonPropertyName("metadata")]
        public object Metadata { get; set; }
        
        [JsonPropertyName("risk_action")]
        public string RiskAction { get; set; }
    }

    public class PaystackAuthorization
    {
        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }
        
        [JsonPropertyName("bin")]
        public string Bin { get; set; }
        
        [JsonPropertyName("last4")]
        public string Last4 { get; set; }
        
        [JsonPropertyName("exp_month")]
        public string ExpMonth { get; set; }
        
        [JsonPropertyName("exp_year")]
        public string ExpYear { get; set; }
        
        [JsonPropertyName("channel")]
        public string Channel { get; set; }
        
        [JsonPropertyName("card_type")]
        public string CardType { get; set; }
        
        [JsonPropertyName("bank")]
        public string Bank { get; set; }
        
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
        
        [JsonPropertyName("brand")]
        public string Brand { get; set; }
        
        [JsonPropertyName("reusable")]
        public bool Reusable { get; set; }
    }

    public class PaystackSplit
    {
        // Empty for now as per JSON
    }

    public class PaystackPlanObject
    {
        // Empty for now as per JSON
    }

    public class PaystackSubaccount
    {
        // Empty for now as per JSON
    }
} 
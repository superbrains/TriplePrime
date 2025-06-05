using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using System.Text.Json;
using TriplePrime.Data.Specifications;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using TriplePrime.Data.Models;
using Microsoft.Extensions.Logging;

namespace TriplePrime.Data.Services
{
    public interface IPaymentService
    {
        Task<PaystackInitializeData> InitializePaystackPaymentAsync(PaystackInitializeRequest request, string userId);
        Task<PaystackVerifyData> VerifyPaystackPaymentAsync(string reference, string userId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly string _paystackSecretKey;
        private readonly string _paystackBaseUrl = "https://api.paystack.co";

        public PaymentService(IUnitOfWork unitOfWork, IConfiguration configuration, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _paystackSecretKey = configuration["Paystack:SecretKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_paystackSecretKey}");
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                payment.CreatedAt = DateTime.UtcNow;
                payment.Status = PaymentStatus.Pending;

                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return payment;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            var spec = new PaymentSpecification(id);
            return await _unitOfWork.Repository<Payment>().GetEntityWithSpec(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyUserFilter(userId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(status);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new PaymentSpecification();
            spec.ApplyDateRangeFilter(startDate, endDate);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task UpdatePaymentStatusAsync(int id, PaymentStatus status, string transactionId = null)
        {
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
            {
                throw new ArgumentException($"Payment with ID {id} not found");
            }

            payment.Status = status;
            payment.TransactionId = transactionId;
            payment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new PaymentSpecification();
            spec.ApplyDateRangeFilter(startDate, endDate);
            spec.ApplyStatusFilter(PaymentStatus.Completed);

            var payments = await _unitOfWork.Repository<Payment>().ListAsync(spec);
            return payments.Sum(p => p.Amount);
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(PaymentStatus.Failed);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(PaymentStatus.Pending);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Initialize Paystack payment
                var paystackResponse = await InitializePaystackPayment(payment);
                if (!paystackResponse.success)
                {
                    throw new Exception($"Paystack payment initialization failed: {paystackResponse.message}");
                }

                payment.Status = PaymentStatus.Pending;
                payment.TransactionId = paystackResponse.data.reference;
                payment.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return payment;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Payment> InitializeSubscriptionAsync(string authCode, string email, decimal amount)
        {
            // Initialize Paystack subscription
            var subscriptionResponse = await InitializePaystackSubscription(authCode, email, amount);
            if (!subscriptionResponse.success)
            {
                throw new Exception($"Paystack subscription initialization failed: {subscriptionResponse.message}");
            }

            var payment = new Payment
            {
                Amount = amount,
                Status = PaymentStatus.Pending,
                TransactionId = subscriptionResponse.data.reference,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return payment;
        }

        public async Task<string> CreatePaystackPlan(decimal amount, string interval = "monthly", string name = null)
        {
            var planName = name ?? $"TriplePrime Savings Plan - ₦{amount}";
            var request = new
            {
                name = planName,
                interval = interval,
                amount = (int)(amount * 100), // Convert to kobo
                currency = "NGN"
            };

            var response = await _httpClient.PostAsync(
                $"{_paystackBaseUrl}/plan",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaystackResponse>(content);
            
            if (!result.success)
            {
                throw new Exception($"Failed to create Paystack plan: {result.message}");
            }

            return result.data.plan_code;
        }

        public async Task<PaystackSubscriptionResponse> CreateSubscription(string customerEmail, string planCode, string authorizationCode, DateTime? startDate = null)
        {
            var request = new
            {
                customer = customerEmail,
                plan = planCode,
                authorization = authorizationCode,
                start_date = (startDate ?? DateTime.UtcNow.AddMonths(1)).ToString("yyyy-MM-dd")
            };

            var response = await _httpClient.PostAsync(
                $"{_paystackBaseUrl}/subscription",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaystackSubscriptionResponse>(content);
            
            if (!result.status)
            {
                throw new Exception($"Failed to create subscription: {result.message}");
            }

            return result;
        }

        private async Task<PaystackResponse> InitializePaystackPayment(Payment payment)
        {
            var request = new
            {
                amount = (int)(payment.Amount * 100), // Convert to kobo
                email = payment.User.Email,
                currency = "NGN",
                callback_url = $"{payment.CallbackUrl}?paymentId={payment.Id}",
                reference = payment.TransactionId
            };

            var response = await _httpClient.PostAsync(
                $"{_paystackBaseUrl}/transaction/initialize",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaystackResponse>(content);
        }

        private async Task<PaystackResponse> InitializePaystackSubscription(string authCode, string email, decimal amount)
        {
            // First, create a plan for this amount if it doesn't exist
            var planCode = await CreatePaystackPlan(amount);
            
            var request = new
            {
                customer = email,
                plan = planCode,
                authorization = authCode,
                start_date = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd") // Start next month
            };

            var response = await _httpClient.PostAsync(
                $"{_paystackBaseUrl}/subscription",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaystackResponse>(content);
        }

        public bool VerifyPaystackWebhook(string signature, string payload)
        {
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(payload))
                return false;

            var computedHash = ComputeHmacSha512(payload, _paystackSecretKey);
            return signature.Equals(computedHash, StringComparison.OrdinalIgnoreCase);
        }

        private string ComputeHmacSha512(string payload, string secretKey)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private async Task<Payment> GetPaymentByReferenceAsync(string reference)
        {
            var spec = new PaymentSpecification();
            spec.ApplyReferenceFilter(reference);
            return await _unitOfWork.Repository<Payment>().GetEntityWithSpec(spec);
        }

        public async Task<Payment> RefundPaymentAsync(int paymentId, decimal amount)
        {
            var payment = await GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                throw new ArgumentException($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Only completed payments can be refunded");
            }

            if (amount > payment.Amount)
            {
                throw new ArgumentException("Refund amount cannot exceed the original payment amount");
            }

            var refund = new Payment
            {
                UserId = payment.UserId,
                Amount = -amount, // Negative amount for refund
                PaymentMethodId = payment.PaymentMethodId,
                Status = PaymentStatus.Completed,
                TransactionId = $"REFUND_{payment.TransactionId}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Payment>().AddAsync(refund);
            await _unitOfWork.SaveChangesAsync();

            return refund;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPaymentMethodAsync(int paymentMethodId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyPaymentMethodFilter(paymentMethodId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByFoodPackAsync(int foodPackId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyFoodPackFilter(foodPackId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<int> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Set any existing payment methods as non-default
                if (paymentMethod.IsDefault)
                {
                    var spec = new PaymentMethodSpecification();
                    spec.ApplyUserFilter(paymentMethod.UserId);
                    var existingMethods = await _unitOfWork.Repository<PaymentMethod>().ListAsync(spec);
                    
                    foreach (var method in existingMethods)
                    {
                        method.IsDefault = false;
                        method.UpdatedAt = DateTime.UtcNow;
                        method.UpdatedBy = paymentMethod.CreatedBy;
                        _unitOfWork.Repository<PaymentMethod>().Update(method);
                    }
                }

                await _unitOfWork.Repository<PaymentMethod>().AddAsync(paymentMethod);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return paymentMethod.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaystackInitializeData> InitializePaystackPaymentAsync(PaystackInitializeRequest request, string userId)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    email = request.Email,
                    amount = request.Amount * 100, // Convert to kobo
                    reference = request.Reference ?? Guid.NewGuid().ToString(),
                    callback_url = request.CallbackUrl,
                    channels = request.Channels,
                    metadata = request.Metadata
                });

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_paystackBaseUrl}/transaction/initialize", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to initialize payment: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaystackInitializeResponse>(responseContent);

                if (!result.Status)
                {
                    throw new Exception(result.Message);
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing Paystack payment: {ex.Message}", ex);
            }
        }

        public async Task<PaystackVerifyData> VerifyPaystackPaymentAsync(string reference, string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_paystackBaseUrl}/transaction/verify/{reference}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to verify payment: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(responseContent);

                if (!result.Status)
                {
                    throw new Exception(result.Message);
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verifying Paystack payment: {ex.Message}", ex);
            }
        }
    }

    public class PaystackResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public PaystackData data { get; set; }
    }

    public class PaystackData
    {
        public string reference { get; set; }
        public string authorization_url { get; set; }
        public string access_code { get; set; }
        public string plan_code { get; set; }
        public string subscription_code { get; set; }
    }

    public class PaystackSubscriptionResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public SubscriptionData data { get; set; }
    }

    public class SubscriptionData
    {
        public string subscription_code { get; set; }
        public string email_token { get; set; }
        public int id { get; set; }
    }

    public class PaystackWebhookData
    {
        public string Event { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public string Reference { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Channel { get; set; }
        public string Currency { get; set; }
        public string Customer { get; set; }
        public string Authorization { get; set; }
        public string SubscriptionCode { get; set; }
        public string PlanCode { get; set; }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TriplePrime.Data.Services;
using TriplePrime.Data.Entities;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using TriplePrime.Data.Models;
using Newtonsoft.Json;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsPlanController : ControllerBase
    {
        private readonly SavingsPlanService _savingsPlanService;
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SavingsPlanController(
            SavingsPlanService savingsPlanService, 
            PaymentService paymentService,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _savingsPlanService = savingsPlanService;
            _paymentService = paymentService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingsPlan([FromBody] CreateSavingsPlanRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Create payment method if automatic payment is selected
            int? paymentMethodId = null;
            if (request.PaymentPreference == "automatic") //&& request.PaymentMethod != null
            {
                try
                {
                    // Verify the transaction with Paystack
                    var paystackSecretKey = _configuration["Paystack:SecretKey"];
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", paystackSecretKey);

                    var verifyResponse = await _httpClient.GetAsync($"https://api.paystack.co/transaction/verify/{request.PaymentReference}");
                    var content = await verifyResponse.Content.ReadAsStringAsync();

                    if (!verifyResponse.IsSuccessStatusCode)
                    {
                        return BadRequest(new { message = "Failed to verify transaction", details = content });
                    }

                    var verifyData = JsonConvert.DeserializeObject<VerificationResponse>(content);
                    
                    // Check if this is a bank transfer
                    if (verifyData.Data.Authorization.Channel?.ToLower() == "bank")
                    {
                        // Bank transfers can only be manual
                        request.PaymentPreference = "manual";
                    }
                    // Check if we have a valid authorization code for card payments
                    else if (string.IsNullOrEmpty(verifyData.Data.Authorization.AuthorizationCode))
                    {
                        // No authorization code, switch to manual payment
                        request.PaymentPreference = "manual";
                    }
                    else
                    {
                        var paymentMethod = new PaymentMethod
                        {
                            UserId = userId,
                            Provider = "Paystack",
                            Type = verifyData.Data.Authorization.Channel,
                            LastFourDigits = verifyData.Data.Authorization.Last4,
                            AuthorizationCode = verifyData.Data.Authorization.AuthorizationCode,
                            CardType = verifyData.Data.Authorization.CardType,
                            Bank = verifyData.Data.Authorization.Bank,
                            IsDefault = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedBy = userId
                        };

                        paymentMethodId = await _paymentService.CreatePaymentMethodAsync(paymentMethod);
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error verifying transaction", error = ex.Message });
                }
            }

            var plan = new SavingsPlan
            {
                UserId = userId,
                FoodPackId = request.FoodPackId,
                TotalAmount = request.TotalAmount,
                MonthlyAmount = request.MonthlyAmount,
                Duration = request.Duration,
                StartDate = request.StartDate,
                PaymentPreference = request.PaymentPreference,
                PaymentFrequency = request.PaymentFrequency ?? "monthly", // Default to monthly if not specified
                PaymentMethodId = paymentMethodId
            };

            var result = await _savingsPlanService.CreateSavingsPlanAsync(plan, request.PaymentReference);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSavingsPlans()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var plans = await _savingsPlanService.GetUserSavingsPlansAsync(userId);
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavingsPlan(int id)
        {
            var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(id);
            if (plan == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (plan.UserId != userId)
                return Forbid();

            return Ok(plan);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(id);
            if (plan == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (plan.UserId != userId)
                return Forbid();

            await _savingsPlanService.UpdateSavingsPlanStatusAsync(id, request.Status);
            return Ok();
        }

        [HttpPut("{id}/payment-preference")]
        public async Task<IActionResult> UpdatePaymentPreference(int id, [FromBody] UpdatePaymentPreferenceRequest request)
        {
            var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(id);
            if (plan == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (plan.UserId != userId)
                return Forbid();

            await _savingsPlanService.UpdatePaymentPreferenceAsync(id, request.Preference, request.PaymentMethod);
            return Ok();
        }

        [HttpPut("{id}/reminders")]
        public async Task<IActionResult> ToggleReminders(int id, [FromBody] ToggleRemindersRequest request)
        {
            var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(id);
            if (plan == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (plan.UserId != userId)
                return Forbid();

            await _savingsPlanService.ToggleRemindersAsync(id, request.Enabled);
            return Ok();
        }

        [HttpGet("upcoming-payments")]
        public async Task<IActionResult> GetUpcomingPayments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var payments = await _savingsPlanService.GetUpcomingPaymentsAsync(userId);
            return Ok(payments);
        }

        //[HttpPost("webhook/paystack")]
        //[AllowAnonymous]
        //public async Task<IActionResult> HandlePaystackWebhook([FromBody] PaystackWebhookRequest request)
        //{
        //    var payload = JsonSerializer.Serialize(request);
        //    if (!_paymentService.VerifyPaystackWebhook(Request.Headers["x-paystack-signature"], payload))
        //        return BadRequest("Invalid signature");

        //    await _paymentService.HandlePaystackWebhookAsync(payload);
        //    return Ok();
        //}

        [HttpPost("{id}/update-payment")]
        public async Task<IActionResult> UpdatePaymentAndPreference(int id, [FromBody] UpdatePaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                // Verify the payment with Paystack
                var paystackSecretKey = _configuration["Paystack:SecretKey"];
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", paystackSecretKey);

                var verifyResponse = await _httpClient.GetAsync($"https://api.paystack.co/transaction/verify/{request.PaymentReference}");
                var content = await verifyResponse.Content.ReadAsStringAsync();

                if (!verifyResponse.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = "Failed to verify transaction", details = content });
                }

                var verifyData = JsonConvert.DeserializeObject<VerificationResponse>(content);

                // Update the savings plan
                var result = await _savingsPlanService.UpdatePaymentAndPreferenceAsync(
                    id,
                    userId,
                    request.PaymentPreference,
                    verifyData.Data.Authorization,
                    request.PaymentReference,
                    request.Amount,
                    request.ScheduleId
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating payment", error = ex.Message });
            }
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSavingsPlans([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string status)
        {
            var plans = await _savingsPlanService.GetAllSavingsPlansForAdminAsync(startDate, endDate, status);
            return Ok(plans);
        }

        [HttpGet("admin/{id}/schedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSavingsPlanSchedule(int id)
        {
            var schedule = await _savingsPlanService.GetSavingsPlanScheduleAsync(id);
            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }
    }

    public class CreateSavingsPlanRequest
    {
        public int FoodPackId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal MonthlyAmount { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public string PaymentPreference { get; set; }
        public string PaymentFrequency { get; set; } // daily, weekly, monthly
        public PaymentMethodRequest PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
    }

    public class PaymentMethodRequest
    {
        public string Type { get; set; }
        public string LastFourDigits { get; set; }
        public string AuthorizationCode { get; set; }
        public string CardType { get; set; }
        public string Bank { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    public class UpdatePaymentPreferenceRequest
    {
        public string Preference { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class ToggleRemindersRequest
    {
        public bool Enabled { get; set; }
    }

    public class PaystackWebhookRequest
    {
        public string Event { get; set; }
        public PaystackWebhookData Data { get; set; }
    }

    public class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackVerifyData Data { get; set; }
    }

    public class PaystackVerifyData
    {
        public PaystackAuthorization Authorization { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public string PaymentPreference { get; set; }
        public string PaymentReference { get; set; }
        public decimal Amount { get; set; }
        public int ScheduleId { get; set; }  // ID of the payment schedule being paid
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : BaseController
    {
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly SavingsPlanService _savingsPlanService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PaymentService paymentService,
            IConfiguration configuration,
            HttpClient httpClient,
            IPaymentMethodService paymentMethodService,
            SavingsPlanService savingsPlanService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _httpClient = httpClient;
            _paymentMethodService = paymentMethodService;
            _savingsPlanService = savingsPlanService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            try
            {
                var createdPayment = await _paymentService.CreatePaymentAsync(payment);
                return HandleResponse(ApiResponse<Payment>.SuccessResponse(createdPayment, "Payment created successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return HandleResponse(ApiResponse<Payment>.ErrorResponse("Payment not found"));
                }
                return HandleResponse(ApiResponse<Payment>.SuccessResponse(payment));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUser(string userId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
                return HandleResponse(ApiResponse<IEnumerable<Payment>>.SuccessResponse(payments));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetPaymentsByStatus(PaymentStatus status)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByStatusAsync(status);
                return HandleResponse(ApiResponse<IEnumerable<Payment>>.SuccessResponse(payments));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatus status)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return HandleResponse(ApiResponse.ErrorResponse("User not authenticated"));
                }

                await _paymentService.UpdatePaymentStatusAsync(id, status);
                return HandleResponse(ApiResponse.SuccessResponse("Payment status updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("process/{id}")]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return HandleResponse(ApiResponse<Payment>.ErrorResponse("Payment not found"));
                }
                
                var processedPayment = await _paymentService.ProcessPaymentAsync(payment);
                return HandleResponse(ApiResponse<Payment>.SuccessResponse(processedPayment, "Payment processed successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("refund/{id}")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] decimal amount)
        {
            try
            {
                var refund = await _paymentService.RefundPaymentAsync(id, amount);
                return HandleResponse(ApiResponse<Payment>.SuccessResponse(refund, "Payment refunded successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("method/{paymentMethodId}")]
        public async Task<IActionResult> GetPaymentsByPaymentMethod(int paymentMethodId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByPaymentMethodAsync(paymentMethodId);
                return HandleResponse(ApiResponse<IEnumerable<Payment>>.SuccessResponse(payments));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("food-pack/{foodPackId}")]
        public async Task<IActionResult> GetPaymentsByFoodPack(int foodPackId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByFoodPackAsync(foodPackId);
                return HandleResponse(ApiResponse<IEnumerable<Payment>>.SuccessResponse(payments));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("verify/{reference}")]
        public async Task<IActionResult> VerifyTransaction(string reference)
        {
            try
            {
                var paystackSecretKey = _configuration["Paystack:SecretKey"];
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", paystackSecretKey);

                var response = await _httpClient.GetAsync($"https://api.paystack.co/transaction/verify/{reference}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = "Failed to verify transaction", details = content });
                }

                var result = JsonSerializer.Deserialize<object>(content);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error verifying transaction", error = ex.Message });
            }
        }

        [HttpPost("initialize-paystack")]
        public async Task<IActionResult> InitializePaystackPayment([FromBody] PaystackInitializeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _paymentService.InitializePaystackPaymentAsync(request, userId);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("verify-paystack/{reference}")]
        public async Task<IActionResult> VerifyPaystackPayment(string reference)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _paymentService.VerifyPaystackPaymentAsync(reference, userId);
                
                // If payment is successful and it's an automatic payment, save the payment method
                if (response.Status == "success" && response.Metadata?.CustomFields?.Any(f => f.VariableName == "is_automatic" && f.Value?.ToString()?.ToLower() == "true") == true)
                {
                    await _paymentMethodService.SavePaystackPaymentMethodAsync(
                        userId,
                        response.Authorization,
                        response.Customer,
                        response.Channel
                    );
                }

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandlePaystackWebhook()
        {
            try
            {
                // Read the request body
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                // Verify the webhook signature
                var paystackSignature = Request.Headers["x-paystack-signature"].ToString();
                var secretKey = _configuration["Paystack:SecretKey"];
                var computedSignature = BitConverter
                    .ToString(System.Security.Cryptography.HMACSHA512.HashData(
                        System.Text.Encoding.UTF8.GetBytes(secretKey),
                        System.Text.Encoding.UTF8.GetBytes(requestBody)))
                    .Replace("-", "")
                    .ToLower();

                if (computedSignature != paystackSignature.ToLower())
                {
                    return BadRequest(new { error = "Invalid signature" });
                }

                // Parse the webhook event
                var webhookEvent = JsonSerializer.Deserialize<PaystackWebhookEvent>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Process the webhook event
                await _savingsPlanService.HandlePaystackWebhookAsync(webhookEvent);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the error but return 200 OK to prevent Paystack from retrying
                _logger.LogError(ex, "Error processing Paystack webhook");
                return Ok();
            }
        }
    }
} 
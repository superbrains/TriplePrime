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
using System.Linq;
using System.Collections.Generic;
using TriplePrime.Data.Specifications;
using TriplePrime.Data.Interfaces;
using Microsoft.Extensions.Logging;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SavingsPlanController> _logger;

        public SavingsPlanController(
            SavingsPlanService savingsPlanService, 
            PaymentService paymentService,
            IConfiguration configuration,
            HttpClient httpClient,
            IUnitOfWork unitOfWork,
            ILogger<SavingsPlanController> logger)
        {
            _savingsPlanService = savingsPlanService;
            _paymentService = paymentService;
            _configuration = configuration;
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingsPlan([FromBody] CreateSavingsPlanRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // If userId is provided in the request and user is admin, use that instead
            if (!string.IsNullOrEmpty(request.UserId))
            {
                var isAdmin = User.IsInRole("Admin");
                if (!isAdmin)
                    return Forbid();
                
                userId = request.UserId;
            }

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

        [HttpGet("admin/defaulters")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDefaulters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var defaulters = await _savingsPlanService.GetDefaultersAsync(pageNumber, pageSize);
            var totalCount = await _savingsPlanService.GetDefaultersCountAsync();
            
            return Ok(new
            {
                Data = defaulters,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpGet("admin/defaulters/{userId}/details")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDefaulterDetails(string userId)
        {
            var spec = new PaymentScheduleSpecification();
            spec.ApplyUserFilter(userId);
            spec.ApplyStatusFilter("Pending");
            spec.ApplyDueDateFilter(DateTime.UtcNow.Date);
            
            var dueSchedules = await _unitOfWork.Repository<PaymentSchedule>().ListAsync(spec);
            var user = await _unitOfWork.Repository<ApplicationUser>().GetEntityWithSpec(new UserSpecification(userId));
            
            if (user == null)
                return NotFound();

            var details = new DefaulterDetails
            {
                UserId = userId,
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                TotalAmountOwed = dueSchedules.Sum(s => s.Amount),
                DuePayments = dueSchedules.Select(s => new DuePaymentInfo
                {
                    ScheduleId = s.Id,
                    DueDate = s.DueDate,
                    Amount = s.Amount,
                    DaysOverdue = (DateTime.UtcNow - s.DueDate).Days
                }).OrderByDescending(p => p.DaysOverdue).ToList()
            };

            return Ok(details);
        }

        [HttpGet("admin/users-without-plans")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersWithoutActivePlans()
        {
            try
            {
                var users = await _savingsPlanService.GetUsersWithoutActivePlansAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users without active plans");
                return StatusCode(500, "An error occurred while fetching users without active plans");
            }
        }

        [HttpPost("{id}/revert-payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevertPayment(int id, [FromBody] RevertPaymentRequest request)
        {
            try
            {
                var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(id);
                if (plan == null)
                    return NotFound();

                var schedule = plan.PaymentSchedules.FirstOrDefault(s => s.Id == request.ScheduleId);
                if (schedule == null)
                    return NotFound();

                if (schedule.Status != "Paid")
                    return BadRequest(new { message = "This payment schedule is not marked as paid" });

                // Update the schedule
                schedule.Status = "Pending";
                schedule.PaymentReference = null;
                schedule.PaidAt = null;
                schedule.UpdatedAt = DateTime.UtcNow;
                schedule.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
                schedule.PaymentReference = plan.PaymentPreference??"";
                // Update the plan
                plan.AmountPaid -= schedule.Amount;
                plan.UpdatedAt = DateTime.UtcNow;
                plan.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";

                // If the plan was completed, change status back to active
                if (plan.Status == "Completed")
                {
                    plan.Status = "Active";
                }

                _unitOfWork.Repository<SavingsPlan>().Update(plan);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Payment reverted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reverting payment", error = ex.Message });
            }
        }

        [HttpPost("{planId}/manual-payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManualPayment(int planId, [FromBody] ManualPaymentRequest request)
        {
            try
            {
                var plan = await _savingsPlanService.GetSavingsPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound("Savings plan not found");

                var schedule = plan.PaymentSchedules.FirstOrDefault(s => s.Id == request.ScheduleId);
                if (schedule == null)
                    return NotFound("Payment schedule not found");

                if (schedule.Status == "Paid")
                    return BadRequest("This payment has already been made");

                // Process the payment
                await _savingsPlanService.ProcessPaymentAsync(planId, request.Amount, $"MANUAL-{DateTime.UtcNow.Ticks}");

                return Ok(new { message = "Payment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating manual payment for plan {PlanId}", planId);
                return StatusCode(500, "An error occurred while updating the payment");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSavingsPlan(int id)
        {
            try
            {
                await _savingsPlanService.DeleteSavingsPlanAsync(id);
                return Ok(new { message = "Savings plan deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting savings plan {PlanId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the savings plan" });
            }
        }

        public class ManualPaymentRequest
        {
            public int ScheduleId { get; set; }
            public decimal Amount { get; set; }
        }
    }

    public class CreateSavingsPlanRequest
    {
        public string UserId { get; set; }  // Optional: Used when admin creates plan for a user
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

    public class DefaulterDetails
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public decimal TotalAmountOwed { get; set; }
        public List<DuePaymentInfo> DuePayments { get; set; }
    }

    public class DuePaymentInfo
    {
        public int ScheduleId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class RevertPaymentRequest
    {
        public int ScheduleId { get; set; }
    }
} 
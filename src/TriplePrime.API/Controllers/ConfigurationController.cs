using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;

namespace TriplePrime.API.Controllers
{
    public class ConfigurationController : BaseController
    {
        private readonly IPenaltyInterestService _penaltyInterestService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(
            IPenaltyInterestService penaltyInterestService,
            ILogger<ConfigurationController> logger)
        {
            _penaltyInterestService = penaltyInterestService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current penalty interest configuration
        /// </summary>
        [HttpGet("penalty-interest")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPenaltyInterestConfig()
        {
            try
            {
                var rate = await _penaltyInterestService.GetDailyInterestRateAsync();
                var enabled = await _penaltyInterestService.IsPenaltyInterestEnabledAsync();

                var response = new
                {
                    DailyRate = rate,
                    DailyRatePercent = rate * 100,
                    Enabled = enabled,
                    Description = $"{rate * 100:F2}% per day"
                };

                return HandleResponse(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving penalty interest configuration");
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Updates the daily penalty interest rate (Admin only)
        /// </summary>
        [HttpPost("penalty-interest")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetPenaltyInterestRate([FromBody] SetPenaltyInterestDto request)
        {
            try
            {
                // Validate rate (between 0.1% and 5% per day)
                if (request.DailyRatePercent < 0.1m || request.DailyRatePercent > 5m)
                {
                    return HandleResponse(
                        ApiResponse.ErrorResponse(
                            "Invalid interest rate. Daily rate must be between 0.1% and 5%"));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
                var rateAsDecimal = request.DailyRatePercent / 100m;

                await _penaltyInterestService.SetDailyInterestRateAsync(rateAsDecimal, userId);

                _logger.LogInformation(
                    "Penalty interest rate updated to {Rate}% by user {UserId}",
                    request.DailyRatePercent,
                    userId);

                var response = new
                {
                    Message = "Penalty interest rate updated successfully",
                    NewRate = rateAsDecimal,
                    NewRatePercent = request.DailyRatePercent,
                    Description = $"{request.DailyRatePercent:F2}% per day"
                };

                return HandleResponse(ApiResponse<object>.SuccessResponse(response));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return HandleResponse(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating penalty interest rate");
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Enables penalty interest accrual (Admin only)
        /// </summary>
        [HttpPost("penalty-interest/enable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnablePenaltyInterest()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";

                await _penaltyInterestService.SetPenaltyInterestEnabledAsync(true, userId);

                _logger.LogInformation(
                    "Penalty interest accrual enabled by user {UserId}",
                    userId);

                return HandleResponse(
                    ApiResponse.SuccessResponse("Penalty interest accrual has been enabled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling penalty interest");
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Disables penalty interest accrual (Admin only)
        /// </summary>
        [HttpPost("penalty-interest/disable")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisablePenaltyInterest()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";

                await _penaltyInterestService.SetPenaltyInterestEnabledAsync(false, userId);

                _logger.LogWarning(
                    "Penalty interest accrual disabled by user {UserId}",
                    userId);

                return HandleResponse(
                    ApiResponse.SuccessResponse("Penalty interest accrual has been disabled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling penalty interest");
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Manually triggers interest calculation for all overdue schedules (Admin only)
        /// </summary>
        [HttpPost("penalty-interest/recalculate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateInterest()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
                _logger.LogInformation("Manual interest recalculation triggered by user {UserId}", userId);

                var result = await _penaltyInterestService.RunCalculationAsync();

                return HandleResponse(ApiResponse<object>.SuccessResponse(new
                {
                    result.SchedulesProcessed,
                    result.SchedulesUpdated,
                    TotalInterestAccrued = result.TotalInterestAccrued,
                    result.Message
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual interest recalculation");
                return HandleException(ex);
            }
        }
    }
}

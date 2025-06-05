using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : BaseController
    {
        private readonly AnalyticsService _analyticsService;

        public AnalyticsController(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetDashboardMetricsAsync();
                return HandleResponse(ApiResponse<DashboardMetrics>.SuccessResponse(metrics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("sales/trends")]
        public async Task<IActionResult> GetSalesTrends(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                if (string.IsNullOrEmpty(groupBy) || !new[] { "day", "week", "month" }.Contains(groupBy.ToLower()))
                {
                    return BadRequest("groupBy must be one of: day, week, month");
                }

                var analytics = await _analyticsService.GetSalesTrendsAsync(startDate, endDate, groupBy);
                return HandleResponse(ApiResponse<SalesTrendAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("users/growth")]
        public async Task<IActionResult> GetUserGrowth(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetUserGrowthAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<UserGrowthAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("foodpacks")]
        public async Task<IActionResult> GetFoodPackAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetFoodPackAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<FoodPackAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<SalesAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("delivery")]
        public async Task<IActionResult> GetDeliveryAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetDeliveryAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<DeliveryAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetUserAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<UserAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("financial")]
        public async Task<IActionResult> GetFinancialAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetFinancialAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<FinancialAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("marketing")]
        public async Task<IActionResult> GetMarketingAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetMarketingAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<MarketingAnalytics>.SuccessResponse(analytics));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
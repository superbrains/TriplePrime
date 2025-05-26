using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
  //  [Authorize]
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
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("food-pack")]
        public async Task<IActionResult> GetFoodPackAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<SalesAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("delivery")]
        public async Task<IActionResult> GetDeliveryAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetDeliveryAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<DeliveryAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetUserAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<UserAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("payment")]
        public async Task<IActionResult> GetPaymentAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetFinancialAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<FinancialAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("referral")]
        public async Task<IActionResult> GetReferralAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetMarketingAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<MarketingAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("marketer")]
        public async Task<IActionResult> GetMarketerAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var analytics = await _analyticsService.GetMarketingAnalyticsAsync(startDate, endDate);
                return HandleResponse(ApiResponse<MarketingAnalytics>.SuccessResponse(analytics));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] Report report)
        {
            try
            {
                var result = await _reportService.CreateReportAsync(report);
                return HandleResponse(ApiResponse<Report>.SuccessResponse(result));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return HandleResponse(ApiResponse<Report>.ErrorResponse("Report not found"));
                }
                return HandleResponse(ApiResponse<Report>.SuccessResponse(report));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReportsByUser(string userId)
        {
            try
            {
                var reports = await _reportService.GetReportsByUserAsync(userId);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetReportsByType(ReportType type)
        {
            try
            {
                var reports = await _reportService.GetReportsByTypeAsync(type);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetReportsByStatus(ReportStatus status)
        {
            try
            {
                var reports = await _reportService.GetReportsByStatusAsync(status);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] UpdateReportStatusRequest request)
        {
            try
            {
                var report = await _reportService.UpdateReportStatusAsync(id, request.Status, request.ErrorMessage);
                if (report == null)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("Report not found"));
                }
                return HandleResponse(ApiResponse<Report>.SuccessResponse(report));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("Report not found"));
                }

                await _reportService.DeleteReportAsync(id);
                return HandleResponse(ApiResponse.SuccessResponse("Report deleted successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetReportsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reports = await _reportService.GetReportsByDateRangeAsync(startDate, endDate);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReports()
        {
            try
            {
                var reports = await _reportService.GetReportsByStatusAsync(ReportStatus.Pending);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedReports()
        {
            try
            {
                var reports = await _reportService.GetReportsByStatusAsync(ReportStatus.Completed);
                return HandleResponse(ApiResponse<IEnumerable<Report>>.SuccessResponse(reports));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    public class UpdateReportStatusRequest
    {
        public ReportStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }
} 
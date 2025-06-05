using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    public class MarketerController : BaseController
    {
        private readonly MarketerService _marketerService;

        public MarketerController(MarketerService marketerService)
        {
            _marketerService = marketerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMarketer([FromBody] CreateMarketerRequest request)
        {
            try
            {
                var result = await _marketerService.CreateMarketerAsync(request);
                return HandleResponse(ApiResponse<MarketerDetails>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMarketerById(int id)
        {
            try
            {
                var marketer = await _marketerService.GetMarketerByIdAsync(id);
                if (marketer == null)
                {
                    return HandleResponse(ApiResponse<MarketerDetails>.ErrorResponse("Marketer not found"));
                }
                return HandleResponse(ApiResponse<MarketerDetails>.SuccessResponse(marketer));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMarketers()
        {
            try
            {
                var marketers = await _marketerService.GetAllMarketersAsync();
                return HandleResponse(ApiResponse<IEnumerable<MarketerDetails>>.SuccessResponse(marketers));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarketer(int id, [FromBody] UpdateMarketerRequest request)
        {
            try
            {
                var result = await _marketerService.UpdateMarketerAsync(id, request);
                return HandleResponse(ApiResponse<MarketerDetails>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPatch("{id}/commission")]
        public async Task<IActionResult> UpdateCommissionRate(int id, [FromBody] decimal newRate)
        {
            try
            {
                var result = await _marketerService.UpdateCommissionRateAsync(id, newRate);
                return HandleResponse(ApiResponse<MarketerDetails>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _marketerService.ChangeStatusAsync(id, isActive);
                return HandleResponse(ApiResponse<MarketerDetails>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}/performance")]
        public async Task<IActionResult> GetMarketerPerformance(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var performance = await _marketerService.GetMarketerPerformanceAsync(id, startDate, endDate);
                return HandleResponse(ApiResponse<MarketerPerformance>.SuccessResponse(performance));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMarketers()
        {
            try
            {
                var marketers = await _marketerService.GetAllMarketersAsync();
                var activeMarketers = marketers.Where(m => m.Status == "active");
                return HandleResponse(ApiResponse<IEnumerable<MarketerDetails>>.SuccessResponse(activeMarketers));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
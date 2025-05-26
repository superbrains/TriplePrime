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
        public async Task<IActionResult> CreateMarketer([FromBody] Marketer marketer)
        {
            try
            {
                var result = await _marketerService.CreateMarketerAsync(marketer);
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(result));
            }
            catch (System.Exception ex)
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
                    return HandleResponse(ApiResponse<Marketer>.ErrorResponse("Marketer not found"));
                }
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(marketer));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMarketerByUserId(string userId)
        {
            try
            {
                var marketer = await _marketerService.GetMarketerByUserIdAsync(userId);
                if (marketer == null)
                {
                    return HandleResponse(ApiResponse<Marketer>.ErrorResponse("Marketer not found"));
                }
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(marketer));
            }
            catch (System.Exception ex)
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
                return HandleResponse(ApiResponse<IEnumerable<Marketer>>.SuccessResponse(marketers));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarketer(int id, [FromBody] Marketer marketer)
        {
            try
            {
                if (id != marketer.Id)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("ID mismatch"));
                }

                var result = await _marketerService.UpdateMarketerAsync(marketer);
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(result));
            }
            catch (System.Exception ex)
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
                var activeMarketers = marketers.Where(m => m.IsActive);
                return HandleResponse(ApiResponse<IEnumerable<Marketer>>.SuccessResponse(activeMarketers));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateMarketer(int id)
        {
            try
            {
                var marketer = await _marketerService.GetMarketerByIdAsync(id);
                if (marketer == null)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("Marketer not found"));
                }

                marketer.IsActive = true;
                var result = await _marketerService.UpdateMarketerAsync(marketer);
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(result));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateMarketer(int id)
        {
            try
            {
                var marketer = await _marketerService.GetMarketerByIdAsync(id);
                if (marketer == null)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("Marketer not found"));
                }

                marketer.IsActive = false;
                var result = await _marketerService.UpdateMarketerAsync(marketer);
                return HandleResponse(ApiResponse<Marketer>.SuccessResponse(result));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
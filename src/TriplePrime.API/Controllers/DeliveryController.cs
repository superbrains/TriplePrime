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
    public class DeliveryController : BaseController
    {
        private readonly DeliveryService _deliveryService;

        public DeliveryController(DeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDelivery([FromBody] Delivery delivery)
        {
            try
            {
                var createdDelivery = await _deliveryService.CreateDeliveryAsync(delivery);
                return HandleResponse(ApiResponse<Delivery>.SuccessResponse(createdDelivery, "Delivery created successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeliveryById(int id)
        {
            try
            {
                var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
                if (delivery == null)
                {
                    return HandleResponse(ApiResponse<Delivery>.ErrorResponse("Delivery not found"));
                }
                return HandleResponse(ApiResponse<Delivery>.SuccessResponse(delivery));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDeliveriesByUser(string userId)
        {
            try
            {
                var deliveries = await _deliveryService.GetDeliveriesByUserAsync(userId);
                return HandleResponse(ApiResponse<IEnumerable<Delivery>>.SuccessResponse(deliveries));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("driver/{driverId}")]
        public async Task<IActionResult> GetDeliveriesByDriver(string driverId)
        {
            try
            {
                var deliveries = await _deliveryService.GetDeliveriesByDriverAsync(driverId);
                return HandleResponse(ApiResponse<IEnumerable<Delivery>>.SuccessResponse(deliveries));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("address/{addressId}")]
        public async Task<IActionResult> GetDeliveriesByAddress(int addressId)
        {
            try
            {
                var deliveries = await _deliveryService.GetDeliveriesByAddressAsync(addressId);
                return HandleResponse(ApiResponse<IEnumerable<Delivery>>.SuccessResponse(deliveries));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("food-pack/{foodPackId}")]
        public async Task<IActionResult> GetDeliveriesByFoodPack(int foodPackId)
        {
            try
            {
                var deliveries = await _deliveryService.GetDeliveriesByFoodPackAsync(foodPackId);
                return HandleResponse(ApiResponse<IEnumerable<Delivery>>.SuccessResponse(deliveries));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateDeliveryStatus(int id, [FromBody] DeliveryStatus status)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return HandleResponse(ApiResponse.ErrorResponse("User not authenticated"));
                }

                await _deliveryService.UpdateDeliveryStatusAsync(id, status, userId);
                return HandleResponse(ApiResponse.SuccessResponse("Delivery status updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/driver")]
        public async Task<IActionResult> AssignDriver(int id, [FromBody] string driverId)
        {
            try
            {
                await _deliveryService.AssignDriverAsync(id, driverId);
                return HandleResponse(ApiResponse.SuccessResponse("Driver assigned successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/address")]
        public async Task<IActionResult> UpdateDeliveryAddress(int id, [FromBody] int addressId)
        {
            try
            {
                await _deliveryService.UpdateDeliveryAddressAsync(id, addressId);
                return HandleResponse(ApiResponse.SuccessResponse("Delivery address updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
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
    public class PaymentController : BaseController
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
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
    }
} 
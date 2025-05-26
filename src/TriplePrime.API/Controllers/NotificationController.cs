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
    public class NotificationController : BaseController
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return HandleResponse(ApiResponse<Notification>.ErrorResponse("Notification not found"));
                }
                return HandleResponse(ApiResponse<Notification>.SuccessResponse(notification));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotificationsByUser(string userId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByUserAsync(userId);
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetNotificationsByType(NotificationType type)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByTypeAsync(type);
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetNotificationsByStatus(NotificationStatus status)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByStatusAsync(status);
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusRequest request)
        {
            try
            {
                var notification = await _notificationService.UpdateNotificationStatusAsync(id, request.Status, request.ErrorMessage);
                if (notification == null)
                {
                    return HandleResponse(ApiResponse<Notification>.ErrorResponse("Notification not found"));
                }
                return HandleResponse(ApiResponse<Notification>.SuccessResponse(notification));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var notification = await _notificationService.MarkNotificationAsReadAsync(id);
                if (notification == null)
                {
                    return HandleResponse(ApiResponse<Notification>.ErrorResponse("Notification not found"));
                }
                return HandleResponse(ApiResponse<Notification>.SuccessResponse(notification));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                if (!result)
                {
                    return HandleResponse(ApiResponse.ErrorResponse("Failed to delete notification"));
                }
                return HandleResponse(ApiResponse.SuccessResponse("Notification deleted successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetNotificationsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByDateRangeAsync(startDate, endDate);
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetPendingNotificationsAsync();
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("failed")]
        public async Task<IActionResult> GetFailedNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetFailedNotificationsAsync();
                return HandleResponse(ApiResponse<IEnumerable<Notification>>.SuccessResponse(notifications));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("process-pending")]
        public async Task<IActionResult> ProcessPendingNotifications()
        {
            try
            {
                await _notificationService.ProcessPendingNotificationsAsync();
                return HandleResponse(ApiResponse.SuccessResponse("Pending notifications processed successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("email")]
        public async Task<IActionResult> SendEmailNotification([FromBody] EmailNotificationRequest request)
        {
            try
            {
                await _notificationService.SendEmailNotificationAsync(request.To, request.Subject, request.Body);
                return HandleResponse(ApiResponse.SuccessResponse("Email notification sent successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("sms")]
        public async Task<IActionResult> SendSmsNotification([FromBody] SmsNotificationRequest request)
        {
            try
            {
                await _notificationService.SendSmsNotificationAsync(request.To, request.Message);
                return HandleResponse(ApiResponse.SuccessResponse("SMS notification sent successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    public class UpdateNotificationStatusRequest
    {
        public NotificationStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class EmailNotificationRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SmsNotificationRequest
    {
        public string To { get; set; }
        public string Message { get; set; }
    }
} 
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TriplePrime.API.Services
{
    public class NotificationService
    {
        private readonly IEmailService _emailService;
        private readonly PushNotificationService _pushNotificationService;
        private readonly UserService _userService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IEmailService emailService,
            PushNotificationService pushNotificationService,
            UserService userService,
            ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Sends both email and push notification to a single user
        /// </summary>
        public async Task SendEmailAndPushNotificationAsync(
            string userEmail,
            string emailSubject,
            string emailBody,
            string pushTitle,
            string pushBody,
            Dictionary<string, string> pushData = null)
        {
            // Send email
            try
            {
                await _emailService.SendEmailAsync(userEmail, emailSubject, emailBody);
                _logger.LogInformation("Email sent successfully to {Email}", userEmail);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", userEmail);
            }

            // Send push notification
            try
            {
                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user != null && !string.IsNullOrWhiteSpace(user.DeviceToken))
                {
                    var deviceTokens = new List<string> { user.DeviceToken };
                    var success = await _pushNotificationService.SendNotificationAsync(deviceTokens, pushTitle, pushBody, pushData);
                    if (success)
                    {
                        _logger.LogInformation("Push notification sent successfully to user {Email}", userEmail);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send push notification to user {Email}", userEmail);
                    }
                }
                else
                {
                    _logger.LogInformation("No device token found for user {Email}, skipping push notification", userEmail);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to {Email}", userEmail);
            }
        }

        /// <summary>
        /// Sends both email and push notification to multiple users
        /// </summary>
        public async Task SendEmailAndPushNotificationToMultipleUsersAsync(
            List<string> userEmails,
            string emailSubject,
            string emailBody,
            string pushTitle,
            string pushBody,
            Dictionary<string, string> pushData = null)
        {
            foreach (var email in userEmails)
            {
                await SendEmailAndPushNotificationAsync(email, emailSubject, emailBody, pushTitle, pushBody, pushData);
            }
        }

        /// <summary>
        /// Sends email only (fallback method for when push notifications are not needed)
        /// </summary>
        public async Task SendEmailOnlyAsync(string userEmail, string subject, string body)
        {
            try
            {
                await _emailService.SendEmailAsync(userEmail, subject, body);
                _logger.LogInformation("Email sent successfully to {Email}", userEmail);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", userEmail);
            }
        }

        /// <summary>
        /// Sends push notification only to users with device tokens
        /// </summary>
        public async Task SendPushNotificationOnlyAsync(
            List<string> userEmails,
            string title,
            string body,
            Dictionary<string, string> data = null)
        {
            try
            {
                var deviceTokens = new List<string>();
                
                foreach (var email in userEmails)
                {
                    var user = await _userService.GetUserByEmailAsync(email);
                    if (user != null && !string.IsNullOrWhiteSpace(user.DeviceToken))
                    {
                        deviceTokens.Add(user.DeviceToken);
                    }
                }

                if (deviceTokens.Any())
                {
                    var success = await _pushNotificationService.SendNotificationAsync(deviceTokens, title, body, data);
                    if (success)
                    {
                        _logger.LogInformation("Push notifications sent successfully to {Count} users", deviceTokens.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send push notifications to some users");
                    }
                }
                else
                {
                    _logger.LogInformation("No device tokens found for provided users, skipping push notifications");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notifications");
            }
        }
    }
}
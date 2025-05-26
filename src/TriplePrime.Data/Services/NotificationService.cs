using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IGenericRepository<Notification> notificationRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.Status = NotificationStatus.Pending;
            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            var spec = new NotificationSpecification(id);
            return await _notificationRepository.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(string userId)
        {
            var spec = new NotificationSpecification(userId);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Notification>> GetNotificationsByTypeAsync(NotificationType type)
        {
            var spec = new NotificationSpecification(type);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Notification>> GetNotificationsByStatusAsync(NotificationStatus status)
        {
            var spec = new NotificationSpecification(status);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task<Notification> UpdateNotificationStatusAsync(int id, NotificationStatus status, string errorMessage = null)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return null;

            notification.Status = status;
            notification.UpdatedAt = DateTime.UtcNow;
            
            if (status == NotificationStatus.Sent)
                notification.SentAt = DateTime.UtcNow;
            else if (status == NotificationStatus.Failed)
                notification.ErrorMessage = errorMessage;

            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> MarkNotificationAsReadAsync(int id)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return null;

            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return false;

            _notificationRepository.Remove(notification);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new NotificationSpecification(startDate, endDate);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync()
        {
            var spec = new NotificationSpecification(NotificationStatus.Pending);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Notification>> GetFailedNotificationsAsync()
        {
            var spec = new NotificationSpecification(NotificationStatus.Failed);
            return (await _notificationRepository.ListAsync(spec)).ToList();
        }

        public async Task ProcessPendingNotificationsAsync()
        {
            var pendingNotifications = await GetPendingNotificationsAsync();
            foreach (var notification in pendingNotifications)
            {
                try
                {
                    switch (notification.Type)
                    {
                        case NotificationType.Email:
                            await _emailService.SendEmailAsync(notification.RecipientEmail, notification.Title, notification.Message);
                            break;
                        case NotificationType.SMS:
                            await _smsService.SendSmsAsync(notification.RecipientPhone, notification.Message);
                            break;
                        case NotificationType.System:
                            // System notifications are handled internally
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported notification type: {notification.Type}");
                    }

                    await UpdateNotificationStatusAsync(notification.Id, NotificationStatus.Sent);
                }
                catch (Exception ex)
                {
                    await UpdateNotificationStatusAsync(notification.Id, NotificationStatus.Failed, ex.Message);
                }
            }
        }

        public async Task SendEmailNotificationAsync(string to, string subject, string body)
        {
            try
            {
                await _emailService.SendEmailAsync(to, subject, body);
                _logger.LogInformation($"Email notification sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification");
                throw;
            }
        }

        public async Task SendSmsNotificationAsync(string to, string message)
        {
            try
            {
                await _smsService.SendSmsAsync(to, message);
                _logger.LogInformation($"SMS notification sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification");
                throw;
            }
        }
    }
} 
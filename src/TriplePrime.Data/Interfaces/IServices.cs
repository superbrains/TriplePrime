using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;

namespace TriplePrime.Data.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(string userId);
        Task<IReadOnlyList<Notification>> GetNotificationsByTypeAsync(NotificationType type);
        Task<IReadOnlyList<Notification>> GetNotificationsByStatusAsync(NotificationStatus status);
        Task<Notification> UpdateNotificationStatusAsync(int id, NotificationStatus status, string errorMessage = null);
        Task<Notification> MarkNotificationAsReadAsync(int id);
        Task<bool> DeleteNotificationAsync(int id);
        Task<IReadOnlyList<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync();
        Task<IReadOnlyList<Notification>> GetFailedNotificationsAsync();
        Task ProcessPendingNotificationsAsync();
        Task SendEmailNotificationAsync(string to, string subject, string body);
        Task SendSmsNotificationAsync(string to, string message);
    }

    // ... existing code ...
} 
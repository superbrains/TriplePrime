using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class NotificationSpecification : BaseSpecification<Notification>
    {
        public NotificationSpecification(int id)
            : base(n => n.Id == id)
        {
            AddInclude(n => n.User);
        }

        public NotificationSpecification(string userId)
            : base(n => n.UserId == userId)
        {
            AddInclude(n => n.User);
        }

        public NotificationSpecification(NotificationType type)
            : base(n => n.Type == type)
        {
            AddInclude(n => n.User);
        }

        public NotificationSpecification(NotificationStatus status)
            : base(n => n.Status == status)
        {
            AddInclude(n => n.User);
        }

        public NotificationSpecification(DateTime startDate, DateTime endDate)
            : base(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
        {
            AddInclude(n => n.User);
        }

        public NotificationSpecification()
        {
            AddInclude(n => n.User);
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            AddCriteria(n => n.Title.Contains(searchTerm) ||
                           n.Message.Contains(searchTerm) ||
                           n.RecipientEmail.Contains(searchTerm) ||
                           n.RecipientPhone.Contains(searchTerm));
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(n => n.CreatedAt);
            else
                ApplyOrderBy(n => n.CreatedAt);
        }

        public void ApplyOrderBySentDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(n => n.SentAt);
            else
                ApplyOrderBy(n => n.SentAt);
        }

        public void ApplyOrderByReadDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(n => n.ReadAt);
            else
                ApplyOrderBy(n => n.ReadAt);
        }
    }
} 
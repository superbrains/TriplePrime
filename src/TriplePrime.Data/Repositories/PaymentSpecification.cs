using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Repositories
{
    public class PaymentSpecification : BaseSpecification<Payment>
    {
        public PaymentSpecification(int id)
            : base(p => p.Id == id)
        {
            AddInclude(p => p.PaymentMethod);
            AddInclude(p => p.FoodPack);
            AddInclude(p => p.User);
        }

        public PaymentSpecification()
            : base()
        {
            AddInclude(p => p.PaymentMethod);
            AddInclude(p => p.FoodPack);
            AddInclude(p => p.User);
        }

        public void ApplyUserFilter(string userId)
        {
            AddCriteria(p => p.UserId == userId);
        }

        public void ApplyStatusFilter(PaymentStatus status)
        {
            AddCriteria(p => p.Status == status);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);
        }

        public void ApplyPaymentMethodFilter(int paymentMethodId)
        {
            AddCriteria(p => p.PaymentMethodId == paymentMethodId);
        }

        public void ApplyFoodPackFilter(int foodPackId)
        {
            AddCriteria(p => p.FoodPackId == foodPackId);
        }

        public void ApplyOrderByAmount(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(p => p.Amount);
            }
            else
            {
                ApplyOrderBy(p => p.Amount);
            }
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(p => p.CreatedAt);
            }
            else
            {
                ApplyOrderBy(p => p.CreatedAt);
            }
        }

        public void ApplyOrderByStatus()
        {
            ApplyOrderBy(p => p.Status);
        }
    }
} 
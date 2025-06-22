using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Specifications
{
    public class SavingsPlanSpecification : BaseSpecification<SavingsPlan>
    {
        public SavingsPlanSpecification()
            : base()
        {
            AddInclude(x => x.PaymentSchedules);
            AddInclude(x => x.User);
            AddInclude(x => x.FoodPack);
            ApplyOrderByDescending(x => x.CreatedAt);
        }

        public SavingsPlanSpecification(int id)
            : base(p => p.Id == id)
        {
            AddInclude(x => x.PaymentSchedules);
            AddInclude(x => x.User);
            AddInclude(x => x.FoodPack);
        }

        public void ApplyUserFilter(string userId)
        {
            AddCriteria(p => p.UserId == userId);
        }

        public void ApplyStatusFilter(string status)
        {
            AddCriteria(p => p.Status == status);
        }

        public void ApplySubscriptionCodeFilter(string subscriptionCode)
        {
            AddCriteria(p => p.SubscriptionCode == subscriptionCode);
        }

        public void ApplyUserEmailFilter(string email)
        {
            AddCriteria(p => p.User.Email == email);
            AddInclude(x => x.User);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(p => p.StartDate >= startDate && p.StartDate <= endDate);
        }

        public void ApplyPaymentReferenceFilter(string reference)
        {
            AddCriteria(p => p.PaymentSchedules.Any(ps => ps.PaymentReference == reference));
            AddInclude(x => x.PaymentSchedules);
        }
    }
} 
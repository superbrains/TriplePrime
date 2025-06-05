using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Interfaces
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
            Criteria = p => p.UserId == userId;
        }

        public void ApplyStatusFilter(string status)
        {
            Criteria = p => p.Status == status;
        }

        public void ApplySubscriptionCodeFilter(string subscriptionCode)
        {
            Criteria = p => p.SubscriptionCode == subscriptionCode;
        }

        public void ApplyUserEmailFilter(string email)
        {
            Criteria = p => p.User.Email == email;
            IncludeStrings.Add("User");
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            Criteria = p => p.StartDate >= startDate && p.StartDate <= endDate;
        }
    }
} 
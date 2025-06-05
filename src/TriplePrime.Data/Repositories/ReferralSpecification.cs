using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class ReferralSpecification : BaseSpecification<Referral>
    {
        public ReferralSpecification(int id)
            : base(r => r.Id == id)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification(string code)
            : base(r => r.ReferralCode == code)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification(string marketerId, bool isMarketer)
            : base(r => r.MarketerId == marketerId)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
            AddInclude(r => r.ReferredUser.SavingsPlans);
            ApplyOrderByDescending(r => r.CreatedAt);
        }

        public ReferralSpecification(string referredUserId, bool isReferredUser, bool dummy)
            : base(r => r.ReferredUserId == referredUserId)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification(DateTime currentDate)
            : base(r => r.Status != ReferralStatus.Completed)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification(DateTime startDate, DateTime endDate)
            : base(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification(string marketerId, DateTime startDate, DateTime endDate)
            : base(r => r.MarketerId == marketerId && r.CreatedAt >= startDate && r.CreatedAt <= endDate)
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public ReferralSpecification()
        {
            AddInclude(r => r.Marketer);
            AddInclude(r => r.ReferredUser);
        }

        public void ApplyStatusFilter(ReferralStatus status)
        {
            Criteria = r => r.Status == status;
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            Criteria = r => r.ReferralCode.Contains(searchTerm);
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(r => r.CreatedAt);
            else
                ApplyOrderBy(r => r.CreatedAt);
        }
    }
} 
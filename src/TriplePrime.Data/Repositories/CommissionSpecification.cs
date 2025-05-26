using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class CommissionSpecification : BaseSpecification<Commission>
    {
        public CommissionSpecification(int id)
            : base(c => c.Id == id)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification(int marketerId, bool isMarketerId)
            : base(c => c.MarketerId == marketerId)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification(int marketerId, CommissionStatus status)
            : base(c => c.MarketerId == marketerId && c.Status == status)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification(int referralId, bool isReferralId, bool dummy)
            : base(c => c.ReferralId == referralId)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification(CommissionStatus status)
            : base(c => c.Status == status)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification(DateTime startDate, DateTime endDate)
            : base(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public CommissionSpecification()
        {
            AddInclude(c => c.Marketer);
            AddInclude(c => c.Referral);
        }

        public void ApplyStatusFilter(CommissionStatus status)
        {
            Criteria = c => c.Status == status;
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            Criteria = c => c.PaymentReference.Contains(searchTerm) ||
                          c.Notes.Contains(searchTerm);
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(c => c.CreatedAt);
            else
                ApplyOrderBy(c => c.CreatedAt);
        }

        public void ApplyOrderByPaymentDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(c => c.PaymentDate);
            else
                ApplyOrderBy(c => c.PaymentDate);
        }

        public void ApplyOrderByAmount(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(c => c.Amount);
            else
                ApplyOrderBy(c => c.Amount);
        }
    }
} 
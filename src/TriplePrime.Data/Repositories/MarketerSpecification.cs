using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class MarketerSpecification : BaseSpecification<Marketer>
    {
        public MarketerSpecification(int id)
            : base(m => m.Id == id)
        {
            AddInclude(m => m.Referrals);
            AddInclude(m => m.Commissions);
            AddInclude(m => m.User);
        }

        public MarketerSpecification(string userId)
            : base(m => m.UserId == userId)
        {
            AddInclude(m => m.Referrals);
            AddInclude(m => m.Commissions);
            AddInclude(m => m.User);
        }

        public MarketerSpecification(bool isActive)
            : base(m => m.IsActive == isActive)
        {
            AddInclude(m => m.Referrals);
            AddInclude(m => m.Commissions);
            AddInclude(m => m.User);
        }

        public MarketerSpecification(DateTime startDate, DateTime endDate)
            : base(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
        {
            AddInclude(m => m.Referrals);
            AddInclude(m => m.Commissions);
            AddInclude(m => m.User);
        }

        public MarketerSpecification()
        {
            AddInclude(m => m.Referrals);
            AddInclude(m => m.Commissions);
            AddInclude(m => m.User);
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            Criteria = m => m.CompanyName.Contains(searchTerm) ||
                          m.PhoneNumber.Contains(searchTerm) ||
                          m.Address.Contains(searchTerm) ||
                          m.City.Contains(searchTerm) ||
                          m.State.Contains(searchTerm) ||
                          m.PostalCode.Contains(searchTerm) ||
                          m.Country.Contains(searchTerm) ||
                          m.Website.Contains(searchTerm) ||
                          m.SocialMediaHandle.Contains(searchTerm);
        }

        public void ApplyOrderByCompanyName(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(m => m.CompanyName);
            else
                ApplyOrderBy(m => m.CompanyName);
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(m => m.CreatedAt);
            else
                ApplyOrderBy(m => m.CreatedAt);
        }

        public void ApplyOrderByTotalCommission(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(m => m.TotalCommissionEarned);
            else
                ApplyOrderBy(m => m.TotalCommissionEarned);
        }
    }
} 
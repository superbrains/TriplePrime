using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class ReportSpecification : BaseSpecification<Report>
    {
        public ReportSpecification(int id)
            : base(r => r.Id == id)
        {
            AddInclude(r => r.User);
        }

        public ReportSpecification(string userId)
            : base(r => r.UserId == userId)
        {
            AddInclude(r => r.User);
        }

        public ReportSpecification(ReportType type)
            : base(r => r.Type == type)
        {
            AddInclude(r => r.User);
        }

        public ReportSpecification(ReportStatus status)
            : base(r => r.Status == status)
        {
            AddInclude(r => r.User);
        }

        public ReportSpecification(DateTime startDate, DateTime endDate)
            : base(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
        {
            AddInclude(r => r.User);
        }

        public ReportSpecification()
        {
            AddInclude(r => r.User);
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            Criteria = r => r.Title.Contains(searchTerm) ||
                          r.Description.Contains(searchTerm) ||
                          r.FileName.Contains(searchTerm);
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(r => r.CreatedAt);
            else
                ApplyOrderBy(r => r.CreatedAt);
        }

        public void ApplyOrderByCompletedDate(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(r => r.CompletedAt);
            else
                ApplyOrderBy(r => r.CompletedAt);
        }

        public void ApplyOrderByTitle(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(r => r.Title);
            else
                ApplyOrderBy(r => r.Title);
        }
    }
} 
using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class LogSpecification : BaseSpecification<LogEntry>
    {
        public LogSpecification(int id)
            : base(l => l.Id == id)
        {
            AddInclude(l => l.User);
        }

        public LogSpecification(string userId)
            : base(l => l.UserId == userId)
        {
            AddInclude(l => l.User);
        }

        public LogSpecification(LogLevel level)
            : base(l => l.Level == level)
        {
            AddInclude(l => l.User);
        }

        public LogSpecification(DateTime startDate, DateTime endDate)
            : base(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
        {
            AddInclude(l => l.User);
        }

        public LogSpecification()
            : base()
        {
            AddInclude(l => l.User);
        }

        public void ApplySearchFilter(string searchTerm)
        {
            var criteria = base.Criteria;
            Expression<Func<LogEntry, bool>> newCriteria = l =>
                l.Message.Contains(searchTerm) ||
                l.Category.Contains(searchTerm) ||
                l.Source.Contains(searchTerm);

            if (criteria == null)
            {
                criteria = newCriteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(LogEntry));
                var combined = Expression.AndAlso(
                    Expression.Invoke(criteria, parameter),
                    Expression.Invoke(newCriteria, parameter));
                criteria = Expression.Lambda<Func<LogEntry, bool>>(combined, parameter);
            }

            base.Criteria = criteria;
        }

        public void ApplyLevelFilter(LogLevel level)
        {
            Criteria = l => l.Level == level;
        }

        public void ApplyCategoryFilter(string category)
        {
            var criteria = base.Criteria;
            Expression<Func<LogEntry, bool>> newCriteria = l => l.Category == category;

            if (criteria == null)
            {
                criteria = newCriteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(LogEntry));
                var combined = Expression.AndAlso(
                    Expression.Invoke(criteria, parameter),
                    Expression.Invoke(newCriteria, parameter));
                criteria = Expression.Lambda<Func<LogEntry, bool>>(combined, parameter);
            }

            base.Criteria = criteria;
        }

        public void ApplySourceFilter(string source)
        {
            var criteria = base.Criteria;
            Expression<Func<LogEntry, bool>> newCriteria = l => l.Source == source;

            if (criteria == null)
            {
                criteria = newCriteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(LogEntry));
                var combined = Expression.AndAlso(
                    Expression.Invoke(criteria, parameter),
                    Expression.Invoke(newCriteria, parameter));
                criteria = Expression.Lambda<Func<LogEntry, bool>>(combined, parameter);
            }

            base.Criteria = criteria;
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            var criteria = base.Criteria;
            Expression<Func<LogEntry, bool>> newCriteria = l => l.CreatedAt >= startDate && l.CreatedAt <= endDate;

            if (criteria == null)
            {
                criteria = newCriteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(LogEntry));
                var combined = Expression.AndAlso(
                    Expression.Invoke(criteria, parameter),
                    Expression.Invoke(newCriteria, parameter));
                criteria = Expression.Lambda<Func<LogEntry, bool>>(combined, parameter);
            }

            base.Criteria = criteria;
        }

        public void ApplyOrderByCreatedDate(bool descending = true)
        {
            if (descending)
                ApplyOrderByDescending(l => l.CreatedAt);
            else
                ApplyOrderBy(l => l.CreatedAt);
        }

        public void ApplyOrderByLevel(bool descending = true)
        {
            if (descending)
                ApplyOrderByDescending(l => l.Level);
            else
                ApplyOrderBy(l => l.Level);
        }
    }
} 
using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class PaymentScheduleSpecification : ISpecification<PaymentSchedule>
    {
        private Expression<Func<PaymentSchedule, bool>> _criteria;
        private List<Expression<Func<PaymentSchedule, object>>> _includes;
        private Expression<Func<PaymentSchedule, object>> _orderBy;
        private Expression<Func<PaymentSchedule, object>> _orderByDescending;
        private List<string> _includeStrings;
        private Expression<Func<PaymentSchedule, object>> _groupBy;
        private int _take;
        private int _skip;
        private bool _isPagingEnabled;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public PaymentScheduleSpecification()
        {
            _includes = new List<Expression<Func<PaymentSchedule, object>>>();
            _includeStrings = new List<string>();
        }

        public Expression<Func<PaymentSchedule, bool>> Criteria => _criteria;
        public List<Expression<Func<PaymentSchedule, object>>> Includes => _includes;
        public Expression<Func<PaymentSchedule, object>> OrderBy => _orderBy;
        public Expression<Func<PaymentSchedule, object>> OrderByDescending => _orderByDescending;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<PaymentSchedule, object>> GroupBy => _groupBy;
        public int Take => _take;
        public int Skip => _skip;
        public bool IsPagingEnabled => _isPagingEnabled;
        public DateTime? StartDate 
        { 
            get => _startDate;
            set => _startDate = value;
        }
        public DateTime? EndDate 
        { 
            get => _endDate;
            set => _endDate = value;
        }

        public void ApplyUserFilter(string userId)
        {
            _criteria = ps => ps.SavingsPlan.UserId == userId;
            _includeStrings.Add("SavingsPlan");
        }

        public void ApplyStatusFilter(string status)
        {
            _criteria = ps => ps.Status == status;
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            _criteria = ps => ps.DueDate >= startDate && ps.DueDate <= endDate;
        }

        public void ApplyDueDateFilter(DateTime dueDate)
        {
            _criteria = ps => ps.DueDate <= dueDate && ps.Status == "Pending";
        }

        public void ApplyPaging(int skip, int take)
        {
            _skip = skip;
            _take = take;
            _isPagingEnabled = true;
        }
    }
} 
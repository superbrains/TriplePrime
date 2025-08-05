using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

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
            _includeStrings.Add("SavingsPlan");
            _includeStrings.Add("SavingsPlan.User");
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
            var userFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.SavingsPlan.UserId == userId);
            _criteria = _criteria == null ? userFilter : CombineCriteria(_criteria, userFilter);
        }

        public void ApplyStatusFilter(string status)
        {
            var statusFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.Status == status);
            _criteria = _criteria == null ? statusFilter : CombineCriteria(_criteria, statusFilter);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            var dateFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.DueDate >= startDate && ps.DueDate <= endDate);
            _criteria = _criteria == null ? dateFilter : CombineCriteria(_criteria, dateFilter);
        }

        public void ApplyDueDateFilter(DateTime dueDate)
        {
            var dueDateFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.DueDate <= dueDate);
            _criteria = _criteria == null ? dueDateFilter : CombineCriteria(_criteria, dueDateFilter);
        }

        public void ApplyScheduleFilter(int scheduleId)
        {
            var scheduleFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.Id == scheduleId);
            _criteria = _criteria == null ? scheduleFilter : CombineCriteria(_criteria, scheduleFilter);
        }

        public void ApplyPlanFilter(int planId)
        {
            var planFilter = (Expression<Func<PaymentSchedule, bool>>)(ps => ps.SavingsPlanId == planId);
            _criteria = _criteria == null ? planFilter : CombineCriteria(_criteria, planFilter);
        }

        public void ApplyPaging(int skip, int take)
        {
            _skip = skip;
            _take = take;
            _isPagingEnabled = true;
        }

        private Expression<Func<PaymentSchedule, bool>> CombineCriteria(
            Expression<Func<PaymentSchedule, bool>> first,
            Expression<Func<PaymentSchedule, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(PaymentSchedule), "ps");
            var body = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );
            return Expression.Lambda<Func<PaymentSchedule, bool>>(body, parameter);
        }
    }
} 
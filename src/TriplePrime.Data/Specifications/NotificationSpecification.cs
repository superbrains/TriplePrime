using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class NotificationSpecification : ISpecification<Notification>
    {
        private Expression<Func<Notification, bool>> _criteria;
        private List<Expression<Func<Notification, object>>> _includes = new List<Expression<Func<Notification, object>>>();
        private List<string> _includeStrings = new List<string>();
        private Expression<Func<Notification, object>> _orderBy;
        private Expression<Func<Notification, object>> _orderByDescending;
        private Expression<Func<Notification, object>> _groupBy;
        private int _take;
        private int _skip;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public Expression<Func<Notification, bool>> Criteria => _criteria;
        public List<Expression<Func<Notification, object>>> Includes => _includes;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<Notification, object>> OrderBy => _orderBy;
        public Expression<Func<Notification, object>> OrderByDescending => _orderByDescending;
        public Expression<Func<Notification, object>> GroupBy => _groupBy;
        public int Take => _take;
        public int Skip => _skip;
        public bool IsPagingEnabled => _take > 0;
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

        public NotificationSpecification()
        {
            _includes.Add(n => n.User);
        }

        public void ApplyUserFilter(string userId)
        {
            _criteria = n => n.UserId == userId;
        }
    }
} 
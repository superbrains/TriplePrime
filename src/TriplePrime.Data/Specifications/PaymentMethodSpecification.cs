using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class PaymentMethodSpecification : ISpecification<PaymentMethod>
    {
        private Expression<Func<PaymentMethod, bool>> _criteria;
        private List<Expression<Func<PaymentMethod, object>>> _includes;
        private Expression<Func<PaymentMethod, object>> _orderBy;
        private Expression<Func<PaymentMethod, object>> _orderByDescending;
        private List<string> _includeStrings;
        private Expression<Func<PaymentMethod, object>> _groupBy;
        private int _take;
        private int _skip;
        private bool _isPagingEnabled;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public PaymentMethodSpecification()
        {
            _includes = new List<Expression<Func<PaymentMethod, object>>>();
            _includeStrings = new List<string>();
        }

        public Expression<Func<PaymentMethod, bool>> Criteria => _criteria;
        public List<Expression<Func<PaymentMethod, object>>> Includes => _includes;
        public Expression<Func<PaymentMethod, object>> OrderBy => _orderBy;
        public Expression<Func<PaymentMethod, object>> OrderByDescending => _orderByDescending;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<PaymentMethod, object>> GroupBy => _groupBy;
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
            _criteria = pm => pm.UserId == userId;
        }

        public void ApplyPaging(int skip, int take)
        {
            _skip = skip;
            _take = take;
            _isPagingEnabled = true;
        }
    }
} 
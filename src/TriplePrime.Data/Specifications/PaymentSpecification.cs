using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class PaymentSpecification : ISpecification<Payment>
    {
        private Expression<Func<Payment, bool>> _criteria;
        private List<Expression<Func<Payment, object>>> _includes;
        private Expression<Func<Payment, object>> _orderBy;
        private Expression<Func<Payment, object>> _orderByDescending;
        private List<string> _includeStrings;
        private Expression<Func<Payment, object>> _groupBy;
        private int _take;
        private int _skip;
        private bool _isPagingEnabled;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public PaymentSpecification()
        {
            _includes = new List<Expression<Func<Payment, object>>>();
            _includeStrings = new List<string>();
        }

        public PaymentSpecification(int id)
        {
            _criteria = p => p.Id == id;
            _includes = new List<Expression<Func<Payment, object>>>();
            _includeStrings = new List<string>();
        }

        public Expression<Func<Payment, bool>> Criteria => _criteria;
        public List<Expression<Func<Payment, object>>> Includes => _includes;
        public Expression<Func<Payment, object>> OrderBy => _orderBy;
        public Expression<Func<Payment, object>> OrderByDescending => _orderByDescending;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<Payment, object>> GroupBy => _groupBy;
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
            _criteria = p => p.UserId == userId;
        }

        public void ApplyStatusFilter(PaymentStatus status)
        {
            _criteria = p => p.Status == status;
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            _criteria = p => p.CreatedAt >= startDate && p.CreatedAt <= endDate;
        }

        public void ApplyPaymentMethodFilter(int paymentMethodId)
        {
            _criteria = p => p.PaymentMethodId == paymentMethodId;
        }

        public void ApplyFoodPackFilter(int foodPackId)
        {
            _criteria = p => p.FoodPackId == foodPackId;
        }

        public void ApplyReferenceFilter(string reference)
        {
            _criteria = p => p.TransactionId == reference;
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class FoodPackPurchaseSpecification : ISpecification<FoodPackPurchase>
    {
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _userId;
        private string _status;
        private List<Expression<Func<FoodPackPurchase, object>>> _includes = new List<Expression<Func<FoodPackPurchase, object>>>();
        private List<string> _includeStrings = new List<string>();
        private Expression<Func<FoodPackPurchase, object>> _orderBy;
        private Expression<Func<FoodPackPurchase, object>> _orderByDescending;
        private Expression<Func<FoodPackPurchase, object>> _groupBy;
        private int _take;
        private int _skip;

        public Expression<Func<FoodPackPurchase, bool>> Criteria => fp =>
            (!_startDate.HasValue || fp.PurchaseDate >= _startDate.Value) &&
            (!_endDate.HasValue || fp.PurchaseDate <= _endDate.Value) &&
            (string.IsNullOrEmpty(_userId) || fp.UserId == _userId) &&
            (string.IsNullOrEmpty(_status) || fp.Status == _status);

        public List<Expression<Func<FoodPackPurchase, object>>> Includes => _includes;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<FoodPackPurchase, object>> OrderBy => _orderBy;
        public Expression<Func<FoodPackPurchase, object>> OrderByDescending => _orderByDescending;
        public Expression<Func<FoodPackPurchase, object>> GroupBy => _groupBy;
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

        public FoodPackPurchaseSpecification()
        {
            _includes.Add(fp => fp.User);
            _includes.Add(fp => fp.FoodPack);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
        }

        public void ApplyUserFilter(string userId)
        {
            _userId = userId;
        }

        public void ApplyStatusFilter(string status)
        {
            _status = status;
        }

        public void AddInclude(Expression<Func<FoodPackPurchase, object>> includeExpression)
        {
            _includes.Add(includeExpression);
        }

        public void AddInclude(string includeString)
        {
            _includeStrings.Add(includeString);
        }

        public void ApplyOrderBy(Expression<Func<FoodPackPurchase, object>> orderByExpression)
        {
            _orderBy = orderByExpression;
        }

        public void ApplyOrderByDescending(Expression<Func<FoodPackPurchase, object>> orderByDescendingExpression)
        {
            _orderByDescending = orderByDescendingExpression;
        }

        public void ApplyGroupBy(Expression<Func<FoodPackPurchase, object>> groupByExpression)
        {
            _groupBy = groupByExpression;
        }

        public void ApplyPaging(int skip, int take)
        {
            _skip = skip;
            _take = take;
        }
    }
} 
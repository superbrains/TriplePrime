using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class FoodPackSpecification : ISpecification<FoodPack>
    {
        private Expression<Func<FoodPack, bool>> _criteria;
        private List<Expression<Func<FoodPack, object>>> _includes = new List<Expression<Func<FoodPack, object>>>();
        private List<string> _includeStrings = new List<string>();
        private Expression<Func<FoodPack, object>> _orderBy;
        private Expression<Func<FoodPack, object>> _orderByDescending;
        private Expression<Func<FoodPack, object>> _groupBy;
        private int _take;
        private int _skip;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public Expression<Func<FoodPack, bool>> Criteria => _criteria;
        public List<Expression<Func<FoodPack, object>>> Includes => _includes;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<FoodPack, object>> OrderBy => _orderBy;
        public Expression<Func<FoodPack, object>> OrderByDescending => _orderByDescending;
        public Expression<Func<FoodPack, object>> GroupBy => _groupBy;
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

        public FoodPackSpecification(int id)
        {
            _criteria = fp => fp.Id == id;
            _includes.Add(fp => fp.Items);
        }

        public FoodPackSpecification(
            string searchQuery = null,
            string category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? available = null,
            bool? featured = null,
            string sortBy = "name",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            _includes.Add(fp => fp.Items);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                AddCriteria(fp => 
                    fp.Name.Contains(searchQuery) || 
                    fp.Description.Contains(searchQuery) || 
                    fp.Category.Contains(searchQuery));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                AddCriteria(fp => fp.Category == category);
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                AddCriteria(fp => fp.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                AddCriteria(fp => fp.Price <= maxPrice.Value);
            }

            // Apply availability filter
            if (available.HasValue)
            {
                AddCriteria(fp => fp.Available == available.Value);
            }

            // Apply featured filter
            if (featured.HasValue)
            {
                AddCriteria(fp => fp.Featured == featured.Value);
            }

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortOrder.ToLower() == "desc")
                        ApplyOrderByDescending(fp => fp.Name);
                    else
                        ApplyOrderBy(fp => fp.Name);
                    break;
                case "price":
                    if (sortOrder.ToLower() == "desc")
                        ApplyOrderByDescending(fp => fp.Price);
                    else
                        ApplyOrderBy(fp => fp.Price);
                    break;
                case "createdat":
                    if (sortOrder.ToLower() == "desc")
                        ApplyOrderByDescending(fp => fp.CreatedAt);
                    else
                        ApplyOrderBy(fp => fp.CreatedAt);
                    break;
                default:
                    ApplyOrderBy(fp => fp.Name);
                    break;
            }

            // Apply pagination
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        public FoodPackSpecification()
        {
            _includes.Add(fp => fp.Items);
        }

        public void AddCriteria(Expression<Func<FoodPack, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (_criteria == null)
            {
                _criteria = criteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(FoodPack), "fp");
                var left = Expression.Invoke(_criteria, parameter);
                var right = Expression.Invoke(criteria, parameter);
                var combined = Expression.AndAlso(left, right);
                _criteria = Expression.Lambda<Func<FoodPack, bool>>(combined, parameter);
            }
        }

        public void AddInclude(Expression<Func<FoodPack, object>> includeExpression)
        {
            _includes.Add(includeExpression);
        }

        public void AddInclude(string includeString)
        {
            _includeStrings.Add(includeString);
        }

        public void ApplyOrderBy(Expression<Func<FoodPack, object>> orderByExpression)
        {
            _orderBy = orderByExpression;
        }

        public void ApplyOrderByDescending(Expression<Func<FoodPack, object>> orderByDescendingExpression)
        {
            _orderByDescending = orderByDescendingExpression;
        }

        public void ApplyGroupBy(Expression<Func<FoodPack, object>> groupByExpression)
        {
            _groupBy = groupByExpression;
        }

        public void ApplyPaging(int skip, int take)
        {
            _skip = skip;
            _take = take;
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class FoodPackPricingSpecification : ISpecification<FoodPackPricing>
    {
        private Expression<Func<FoodPackPricing, bool>> _criteria;
        private List<Expression<Func<FoodPackPricing, object>>> _includes = new List<Expression<Func<FoodPackPricing, object>>>();
        private List<string> _includeStrings = new List<string>();
        private Expression<Func<FoodPackPricing, object>> _orderBy;
        private Expression<Func<FoodPackPricing, object>> _orderByDescending;
        private Expression<Func<FoodPackPricing, object>> _groupBy;
        private int _take;
        private int _skip;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public Expression<Func<FoodPackPricing, bool>> Criteria => _criteria;
        public List<Expression<Func<FoodPackPricing, object>>> Includes => _includes;
        public List<string> IncludeStrings => _includeStrings;
        public Expression<Func<FoodPackPricing, object>> OrderBy => _orderBy;
        public Expression<Func<FoodPackPricing, object>> OrderByDescending => _orderByDescending;
        public Expression<Func<FoodPackPricing, object>> GroupBy => _groupBy;
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

        // Get all pricings for a specific food pack
        public FoodPackPricingSpecification(int foodPackId)
        {
            _criteria = p => p.FoodPackId == foodPackId;
            _orderBy = p => p.DurationMonths;
        }

        // Get specific pricing by food pack and duration
        public FoodPackPricingSpecification(int foodPackId, int durationMonths)
        {
            _criteria = p => p.FoodPackId == foodPackId && p.DurationMonths == durationMonths;
        }

        // Get pricing by ID
        public FoodPackPricingSpecification(int id, bool byId)
        {
            if (byId)
            {
                _criteria = p => p.Id == id;
            }
        }
    }
}

using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Repositories
{
    public class FoodPackSpecification : BaseSpecification<FoodPack>
    {
        public FoodPackSpecification(int id)
            : base(fp => fp.Id == id)
        {
            AddInclude(fp => fp.Items);
            AddInclude(fp => fp.SavingsPlan);
        }

        public FoodPackSpecification(string userId, bool includeItems)
            : base(fp => fp.UserId == userId)
        {
            if (includeItems)
            {
                AddInclude(fp => fp.Items);
            }
            AddInclude(fp => fp.SavingsPlan);
        }

        public FoodPackSpecification(bool includeItems)
            : base()
        {
            if (includeItems)
            {
                AddInclude(fp => fp.Items);
            }
            AddInclude(fp => fp.SavingsPlan);
        }

        public FoodPackSpecification()
            : base()
        {
            AddInclude(fp => fp.Items);
            AddInclude(fp => fp.SavingsPlan);
        }

        public void ApplyStatusFilter(FoodPackStatus status)
        {
            AddCriteria(fp => fp.Status == status);
        }

        public void ApplyCategoryFilter(string category)
        {
            AddCriteria(fp => fp.Category == category);
        }

        public void ApplyPriceRangeFilter(decimal minPrice, decimal maxPrice)
        {
            AddCriteria(fp => fp.Price >= minPrice && fp.Price <= maxPrice);
        }

        public void ApplySearchFilter(string searchTerm)
        {
            AddCriteria(fp => 
                fp.Name.Contains(searchTerm) || 
                fp.Description.Contains(searchTerm) || 
                fp.Category.Contains(searchTerm));
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(fp => fp.CreatedAt >= startDate && fp.CreatedAt <= endDate);
        }

        public void ApplyDeliveryDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(fp => fp.DeliveryDate >= startDate && fp.DeliveryDate <= endDate);
        }

        public void ApplyOrderByPopularity()
        {
            ApplyOrderByDescending(fp => fp.PopularityScore);
        }

        public void ApplyOrderByRating()
        {
            ApplyOrderByDescending(fp => fp.Rating);
        }

        public void ApplyOrderByPrice(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(fp => fp.Price);
            }
            else
            {
                ApplyOrderBy(fp => fp.Price);
            }
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(fp => fp.CreatedAt);
            }
            else
            {
                ApplyOrderBy(fp => fp.CreatedAt);
            }
        }

        public void ApplyOrderByDeliveryDate(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(fp => fp.DeliveryDate);
            }
            else
            {
                ApplyOrderBy(fp => fp.DeliveryDate);
            }
        }

        public void ApplyPagination(int pageNumber, int pageSize)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }

        protected override void AddCriteria(Expression<Func<FoodPack, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (Criteria == null)
            {
                Criteria = criteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(FoodPack));
                
                if (parameter != null)
                {
                    var combined = Expression.AndAlso(
                        Expression.Invoke(Criteria, parameter),
                        Expression.Invoke(criteria, parameter));
                    
                    Criteria = Expression.Lambda<Func<FoodPack, bool>>(combined, parameter);
                }
            }
        }
    }
} 
using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class FoodPackItemSpecification : BaseSpecification<FoodPackItem>
    {
        public FoodPackItemSpecification(int foodPackId)
            : base(item => item.FoodPackId == foodPackId)
        {
        }

        public FoodPackItemSpecification()
            : base()
        {
        }
    }
} 
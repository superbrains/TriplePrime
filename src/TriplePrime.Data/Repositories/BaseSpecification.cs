using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
            Includes = new List<Expression<Func<T, object>>>();
            OrderBy = null;
            OrderByDescending = null;
        }

        protected BaseSpecification()
        {
            Includes = new List<Expression<Func<T, object>>>();
            OrderBy = null;
            OrderByDescending = null;
        }

        public Expression<Func<T, bool>>? Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; }
        public List<string> IncludeStrings { get; } = new List<string>();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public Expression<Func<T, object>>? GroupBy { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            OrderByDescending = null;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
            OrderBy = null;
        }

        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        protected virtual void AddCriteria(Expression<Func<T, bool>> criteria)
        {
            if (Criteria == null)
            {
                Criteria = criteria;
            }
            else
            {
                var parameter = Expression.Parameter(typeof(T));
                var body = Expression.AndAlso(
                    Expression.Invoke(Criteria, parameter),
                    Expression.Invoke(criteria, parameter)
                );
                Criteria = Expression.Lambda<Func<T, bool>>(body, parameter);
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
        Task<int> CountAsync(Expression<Func<T, bool>> expression = null);

        // Specification pattern methods
        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<IEnumerable<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<T> FirstOrDefaultAsync(ISpecification<T> spec);
        Task<T> SingleOrDefaultAsync(ISpecification<T> spec);

        // Complex query methods
        Task<IEnumerable<T>> GetPagedListAsync(ISpecification<T> spec, int pageNumber, int pageSize);
        Task<IEnumerable<TResult>> SelectAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector);
        Task<TResult> MaxAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector);
        Task<TResult> MinAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector);
        Task<double> AverageAsync(ISpecification<T> spec, Expression<Func<T, decimal>> selector);
        Task<decimal> SumAsync(ISpecification<T> spec, Expression<Func<T, decimal>> selector);
    }
} 
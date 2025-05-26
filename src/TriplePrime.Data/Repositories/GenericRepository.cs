using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.CountAsync(expression);
        }

        // Specification Pattern Implementation
        public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> ListAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task<T> FirstOrDefaultAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<T> SingleOrDefaultAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).SingleOrDefaultAsync();
        }

        // Complex Query Methods
        public async Task<IEnumerable<T>> GetPagedListAsync(ISpecification<T> spec, int pageNumber, int pageSize)
        {
            return await ApplySpecification(spec)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<TResult>> SelectAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector)
        {
            return await ApplySpecification(spec)
                .Select(selector)
                .ToListAsync();
        }

        public async Task<TResult> MaxAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector)
        {
            return await ApplySpecification(spec)
                .MaxAsync(selector);
        }

        public async Task<TResult> MinAsync<TResult>(ISpecification<T> spec, Expression<Func<T, TResult>> selector)
        {
            return await ApplySpecification(spec)
                .MinAsync(selector);
        }

        public async Task<double> AverageAsync(ISpecification<T> spec, Expression<Func<T, decimal>> selector)
        {
            return (double)await ApplySpecification(spec)
                .AverageAsync(selector);
        }

        public async Task<decimal> SumAsync(ISpecification<T> spec, Expression<Func<T, decimal>> selector)
        {
            return await ApplySpecification(spec)
                .SumAsync(selector);
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
        }
    }
} 
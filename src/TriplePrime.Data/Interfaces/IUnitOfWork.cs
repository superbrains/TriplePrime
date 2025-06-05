using System;
using System.Threading.Tasks;

namespace TriplePrime.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
       
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
} 
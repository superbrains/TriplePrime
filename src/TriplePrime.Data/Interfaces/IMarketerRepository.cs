using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IMarketerRepository : IGenericRepository<Marketer>
    {
        Task<Marketer> GetByUserIdAsync(string userId);
    }
} 
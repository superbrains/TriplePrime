using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IMarketerService
    {
        Task<Marketer> CreateMarketerAsync(Marketer marketer);
        Task<Marketer> GetMarketerByIdAsync(int id);
        Task<Marketer> GetMarketerByUserIdAsync(string userId);
        Task<IReadOnlyList<Marketer>> GetAllMarketersAsync();
        Task<Marketer> UpdateMarketerAsync(Marketer marketer);
        Task<bool> DeleteMarketerAsync(int id);
        Task<IReadOnlyList<Commission>> GetMarketerCommissionsAsync(int marketerId);
        Task<decimal> CalculateCommissionAsync(int marketerId, decimal amount);
        Task<bool> ProcessCommissionAsync(int marketerId, decimal amount);
    }
} 
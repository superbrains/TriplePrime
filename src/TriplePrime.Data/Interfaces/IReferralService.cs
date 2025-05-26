using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IReferralService
    {
        Task<Referral> CreateReferralAsync(string marketerId, string referredUserId);
        Task<Referral> GetReferralByIdAsync(int id);
        Task<IReadOnlyList<Referral>> GetReferralsByMarketerAsync(string marketerId);
        Task<IReadOnlyList<Referral>> GetReferralsByUserAsync(string userId);
        Task<string> GenerateReferralCodeAsync(string marketerId);
        Task<bool> ValidateReferralCodeAsync(string referralCode);
        Task<bool> ProcessReferralAsync(string referralCode, string userId);
    }
} 
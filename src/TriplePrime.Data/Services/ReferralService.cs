//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using TriplePrime.Data.Entities;
//using TriplePrime.Data.Interfaces;
//using TriplePrime.Data.Repositories;

//namespace TriplePrime.Data.Services
//{
//    public class ReferralService
//    {
//        private readonly IGenericRepository<Referral> _referralRepository;
//        private readonly IUnitOfWork _unitOfWork;

//        public ReferralService(IGenericRepository<Referral> referralRepository, IUnitOfWork unitOfWork)
//        {
//            _referralRepository = referralRepository;
//            _unitOfWork = unitOfWork;
//        }

//        public async Task<Referral> CreateReferralAsync(Referral referral)
//        {
//            referral.CreatedAt = DateTime.UtcNow;
//            referral.Status = ReferralStatus.Pending;
//            await _referralRepository.AddAsync(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return referral;
//        }

//        public async Task<Referral> GetReferralByIdAsync(int id)
//        {
//            var spec = new ReferralSpecification(id);
//            return await _referralRepository.GetEntityWithSpec(spec);
//        }

//        public async Task<Referral> GetReferralByCodeAsync(string code)
//        {
//            var spec = new ReferralSpecification(code);
//            return await _referralRepository.GetEntityWithSpec(spec);
//        }

//        public async Task<IReadOnlyList<Referral>> GetReferralsByMarketerAsync(int marketerId)
//        {
//            var spec = new ReferralSpecification(marketerId);
//            return (await _referralRepository.ListAsync(spec)).ToList();
//        }

//        public async Task<IReadOnlyList<Referral>> GetReferralsByUserAsync(string userId)
//        {
//            var spec = new ReferralSpecification(userId);
//            return (await _referralRepository.ListAsync(spec)).ToList();
//        }

//        public async Task<Referral> UpdateReferralStatusAsync(int id, ReferralStatus status)
//        {
//            var referral = await GetReferralByIdAsync(id);
//            if (referral == null) return null;

//            referral.Status = status;
//            referral.UpdatedAt = DateTime.UtcNow;
//            _referralRepository.Update(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return referral;
//        }

//        public async Task<Referral> ActivateReferralAsync(int id)
//        {
//            var referral = await GetReferralByIdAsync(id);
//            if (referral == null) return null;

//            referral.Status = ReferralStatus.Active;
//            referral.UpdatedAt = DateTime.UtcNow;
//            _referralRepository.Update(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return referral;
//        }

//        public async Task<Referral> CompleteReferralAsync(int id, decimal totalSpent)
//        {
//            var referral = await GetReferralByIdAsync(id);
//            if (referral == null) return null;

//            referral.Status = ReferralStatus.Completed;
//            referral.UpdatedAt = DateTime.UtcNow;
//            _referralRepository.Update(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return referral;
//        }

//        public async Task<IReadOnlyList<Referral>> GetExpiredReferralsAsync()
//        {
//            var spec = new ReferralSpecification(DateTime.UtcNow);
//            return (await _referralRepository.ListAsync(spec)).ToList();
//        }

//        public async Task<IReadOnlyList<Referral>> GetReferralsByDateRangeAsync(DateTime startDate, DateTime endDate)
//        {
//            var spec = new ReferralSpecification(startDate, endDate);
//            return (await _referralRepository.ListAsync(spec)).ToList();
//        }

//        public async Task<string> GenerateReferralCodeAsync(int marketerId)
//        {
//            var code = Guid.NewGuid().ToString("N").Substring(0, 8);
//            var referral = new Referral
//            {
//                MarketerId = marketerId,
//                ReferralCode = code,
//                Status = ReferralStatus.Pending,
//                CreatedAt = DateTime.UtcNow
//            };
            
//            await _referralRepository.AddAsync(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return code;
//        }

//        public async Task<bool> ValidateReferralCodeAsync(string referralCode)
//        {
//            var referral = await GetReferralByCodeAsync(referralCode);
//            return referral != null && referral.Status == ReferralStatus.Pending;
//        }

//        public async Task<bool> ProcessReferralAsync(string referralCode, string userId)
//        {
//            var referral = await GetReferralByCodeAsync(referralCode);
//            if (referral == null || referral.Status != ReferralStatus.Pending)
//                return false;

//            referral.ReferredUserId = userId;
//            referral.Status = ReferralStatus.Completed;
//            referral.UpdatedAt = DateTime.UtcNow;
            
//            _referralRepository.Update(referral);
//            await _unitOfWork.SaveChangesAsync();
//            return true;
//        }
//    }
//} 
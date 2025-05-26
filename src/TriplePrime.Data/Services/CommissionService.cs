using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class CommissionService
    {
        private readonly IGenericRepository<Commission> _commissionRepository;
        private readonly IGenericRepository<Marketer> _marketerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CommissionService(
            IGenericRepository<Commission> commissionRepository,
            IGenericRepository<Marketer> marketerRepository,
            IUnitOfWork unitOfWork)
        {
            _commissionRepository = commissionRepository;
            _marketerRepository = marketerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Commission> CreateCommissionAsync(Commission commission)
        {
            commission.CreatedAt = DateTime.UtcNow;
            commission.Status = CommissionStatus.Pending;
            await _commissionRepository.AddAsync(commission);
            await _unitOfWork.SaveChangesAsync();
            return commission;
        }

        public async Task<Commission> GetCommissionByIdAsync(int id)
        {
            var spec = new CommissionSpecification(id);
            return await _commissionRepository.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Commission>> GetCommissionsByMarketerAsync(int marketerId)
        {
            var spec = new CommissionSpecification(marketerId, true);
            var commissions = await _commissionRepository.ListAsync(spec);
            return commissions.ToList();
        }

        public async Task<IReadOnlyList<Commission>> GetCommissionsByReferralAsync(int referralId)
        {
            var spec = new CommissionSpecification(referralId, true, true);
            var commissions = await _commissionRepository.ListAsync(spec);
            return commissions.ToList();
        }

        public async Task<Commission> UpdateCommissionStatusAsync(int id, CommissionStatus status)
        {
            var commission = await GetCommissionByIdAsync(id);
            if (commission == null) return null;

            commission.Status = status;
            commission.UpdatedAt = DateTime.UtcNow;
            _commissionRepository.Update(commission);
            await _unitOfWork.SaveChangesAsync();
            return commission;
        }

        public async Task<Commission> ApproveCommissionAsync(int id)
        {
            var commission = await GetCommissionByIdAsync(id);
            if (commission == null) return null;

            commission.Status = CommissionStatus.Approved;
            commission.UpdatedAt = DateTime.UtcNow;
            _commissionRepository.Update(commission);
            await _unitOfWork.SaveChangesAsync();
            return commission;
        }

        public async Task<Commission> PayCommissionAsync(int id, string paymentReference)
        {
            var commission = await GetCommissionByIdAsync(id);
            if (commission == null) return null;

            commission.Status = CommissionStatus.Paid;
            commission.PaymentDate = DateTime.UtcNow;
            commission.PaymentReference = paymentReference;
            commission.UpdatedAt = DateTime.UtcNow;

            // Update marketer's total commission and current balance
            var marketer = await _marketerRepository.GetByIdAsync(commission.MarketerId);
            if (marketer != null)
            {
                marketer.TotalCommissionEarned += commission.Amount;
                marketer.CurrentBalance += commission.Amount;
                marketer.UpdatedAt = DateTime.UtcNow;
                _marketerRepository.Update(marketer);
            }

            _commissionRepository.Update(commission);
            await _unitOfWork.SaveChangesAsync();
            return commission;
        }

        public async Task<IReadOnlyList<Commission>> GetPendingCommissionsAsync()
        {
            var spec = new CommissionSpecification(CommissionStatus.Pending);
            var commissions = await _commissionRepository.ListAsync(spec);
            return commissions.ToList();
        }

        public async Task<IReadOnlyList<Commission>> GetCommissionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new CommissionSpecification(startDate, endDate);
            var commissions = await _commissionRepository.ListAsync(spec);
            return commissions.ToList();
        }

        public async Task<decimal> CalculateTotalCommissionsByMarketerAsync(int marketerId)
        {
            var spec = new CommissionSpecification(marketerId, true);
            return await _commissionRepository.SumAsync(spec, c => c.Amount);
        }

        public async Task<decimal> CalculatePendingCommissionsByMarketerAsync(int marketerId)
        {
            var spec = new CommissionSpecification(marketerId, CommissionStatus.Pending);
            return await _commissionRepository.SumAsync(spec, c => c.Amount);
        }
    }
} 
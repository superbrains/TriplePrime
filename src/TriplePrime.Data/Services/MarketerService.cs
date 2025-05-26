using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class MarketerService
    {
        private readonly IGenericRepository<Marketer> _marketerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarketerService(IGenericRepository<Marketer> marketerRepository, IUnitOfWork unitOfWork)
        {
            _marketerRepository = marketerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Marketer> CreateMarketerAsync(Marketer marketer)
        {
            marketer.CreatedAt = DateTime.UtcNow;
            await _marketerRepository.AddAsync(marketer);
            await _unitOfWork.SaveChangesAsync();
            return marketer;
        }

        public async Task<Marketer> GetMarketerByIdAsync(int id)
        {
            var spec = new MarketerSpecification(id);
            return await _marketerRepository.GetEntityWithSpec(spec);
        }

        public async Task<Marketer> GetMarketerByUserIdAsync(string userId)
        {
            var spec = new MarketerSpecification(userId);
            return await _marketerRepository.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Marketer>> GetAllMarketersAsync()
        {
            var spec = new MarketerSpecification();
            return (await _marketerRepository.ListAsync(spec)).ToList();
        }

        public async Task<Marketer> UpdateMarketerAsync(Marketer marketer)
        {
            marketer.UpdatedAt = DateTime.UtcNow;
            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();
            return marketer;
        }

        public async Task<bool> DeactivateMarketerAsync(int id)
        {
            var marketer = await GetMarketerByIdAsync(id);
            if (marketer == null) return false;

            marketer.IsActive = false;
            marketer.UpdatedAt = DateTime.UtcNow;
            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateMarketerAsync(int id)
        {
            var marketer = await GetMarketerByIdAsync(id);
            if (marketer == null) return false;

            marketer.IsActive = true;
            marketer.UpdatedAt = DateTime.UtcNow;
            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<Marketer>> GetActiveMarketersAsync()
        {
            var spec = new MarketerSpecification(isActive: true);
            return (await _marketerRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Marketer>> GetMarketersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new MarketerSpecification(startDate, endDate);
            return (await _marketerRepository.ListAsync(spec)).ToList();
        }
    }
} 
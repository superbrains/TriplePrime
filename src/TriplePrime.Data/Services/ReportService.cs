using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class ReportService
    {
        private readonly IGenericRepository<Report> _reportRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IGenericRepository<Report> reportRepository, IUnitOfWork unitOfWork)
        {
            _reportRepository = reportRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            report.CreatedAt = DateTime.UtcNow;
            report.Status = ReportStatus.Pending;
            await _reportRepository.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
            return report;
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            var spec = new ReportSpecification(id);
            return await _reportRepository.GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Report>> GetReportsByUserAsync(string userId)
        {
            var spec = new ReportSpecification(userId);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Report>> GetReportsByTypeAsync(ReportType type)
        {
            var spec = new ReportSpecification(type);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Report>> GetReportsByStatusAsync(ReportStatus status)
        {
            var spec = new ReportSpecification(status);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<Report> UpdateReportStatusAsync(int id, ReportStatus status, string errorMessage = null)
        {
            var report = await GetReportByIdAsync(id);
            if (report == null) return null;

            report.Status = status;
            report.UpdatedAt = DateTime.UtcNow;
            
            if (status == ReportStatus.Completed)
                report.CompletedAt = DateTime.UtcNow;
            else if (status == ReportStatus.Failed)
                report.ErrorMessage = errorMessage;

            _reportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();
            return report;
        }

        public async Task<Report> UpdateReportFileAsync(int id, string fileName, string filePath)
        {
            var report = await GetReportByIdAsync(id);
            if (report == null) return null;

            report.FileName = fileName;
            report.FilePath = filePath;
            report.UpdatedAt = DateTime.UtcNow;

            _reportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();
            return report;
        }

        public async Task<IReadOnlyList<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new ReportSpecification(startDate, endDate);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Report>> GetPendingReportsAsync()
        {
            var spec = new ReportSpecification(ReportStatus.Pending);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Report>> GetFailedReportsAsync()
        {
            var spec = new ReportSpecification(ReportStatus.Failed);
            return (await _reportRepository.ListAsync(spec)).ToList();
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await GetReportByIdAsync(id);
            if (report == null) return false;

            _reportRepository.Remove(report);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 
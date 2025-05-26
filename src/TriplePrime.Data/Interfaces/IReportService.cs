using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IReportService
    {
        Task<Report> GenerateReportAsync(string userId, string title, string description, string parameters);
        Task<Report> GetReportByIdAsync(int id);
        Task<IReadOnlyList<Report>> GetReportsByUserAsync(string userId);
        Task<IReadOnlyList<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> DeleteReportAsync(int id);
        Task<string> ExportReportAsync(int id, string format);
        Task<bool> ScheduleReportAsync(string userId, string title, string description, string parameters, string schedule);
    }
} 
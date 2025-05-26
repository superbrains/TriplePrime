using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface ILoggingService
    {
        Task<LogEntry> LogInformationAsync(string message, string category = null);
        Task<LogEntry> LogWarningAsync(string message, string category = null);
        Task<LogEntry> LogErrorAsync(string message, Exception exception = null, string category = null);
        Task<LogEntry> LogDebugAsync(string message, string category = null);
        Task<LogEntry> LogCriticalAsync(string message, Exception exception = null, string category = null);
        Task<IReadOnlyList<LogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<LogEntry>> GetLogsByCategoryAsync(string category);
        Task<IReadOnlyList<LogEntry>> GetLogsByUserAsync(string userId);
        Task<bool> ClearLogsAsync(DateTime before);
    }
} 
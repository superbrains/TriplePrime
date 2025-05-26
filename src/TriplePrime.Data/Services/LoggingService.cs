using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using LogLevel = TriplePrime.Data.Entities.LogLevel;

namespace TriplePrime.Data.Services
{
    public class LoggingService
    {
        private readonly IGenericRepository<LogEntry> _logRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(
            IGenericRepository<LogEntry> logRepository,
            IUnitOfWork unitOfWork,
            ILogger<LoggingService> logger)
        {
            _logRepository = logRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LogEntry> LogInformationAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Information, message, category, source, exception);
        }

        public async Task<LogEntry> LogWarningAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Warning, message, category, source, exception);
        }

        public async Task<LogEntry> LogErrorAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Error, message, category, source, exception);
        }

        public async Task<LogEntry> LogCriticalAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Critical, message, category, source, exception);
        }

        public async Task<LogEntry> LogDebugAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Debug, message, category, source, exception);
        }

        public async Task<LogEntry> LogTraceAsync(string message, string category, string source, Exception exception = null)
        {
            return await LogAsync(Microsoft.Extensions.Logging.LogLevel.Trace, message, category, source, exception);
        }

        private async Task<LogEntry> LogAsync(
            Microsoft.Extensions.Logging.LogLevel level,
            string message,
            string category,
            string source,
            Exception exception = null)
        {
            var entry = new LogEntry
            {
                Level = MapLogLevel(level),
                Message = message,
                Category = category,
                Source = source,
                CreatedAt = DateTime.UtcNow
            };

            if (exception != null)
            {
                entry.Exception = exception.ToString();
                entry.StackTrace = exception.StackTrace;
            }

            await _logRepository.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            // Also log to standard logging system
            _logger.Log(level, exception, $"[{category}] {message}");

            return entry;
        }

        private static LogLevel MapLogLevel(Microsoft.Extensions.Logging.LogLevel level)
        {
            return level switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
                Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Information,
                Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }

        public async Task<IReadOnlyList<LogEntry>> GetLogsAsync()
        {
            var logs = await _logRepository.GetAllAsync();
            return logs.ToList();
        }

        public async Task<LogEntry> GetLogByIdAsync(int id)
        {
            var logs = await _logRepository.FindAsync(l => l.Id == id);
            return logs.FirstOrDefault();
        }

        public async Task<IReadOnlyList<LogEntry>> GetLogsByLevelAsync(LogLevel level)
        {
            var logs = await _logRepository.FindAsync(l => l.Level == level);
            return logs.ToList();
        }

        public async Task<IReadOnlyList<LogEntry>> GetLogsByDateRangeAsync(DateTime start, DateTime end)
        {
            var logs = await _logRepository.FindAsync(l => l.CreatedAt >= start && l.CreatedAt <= end);
            return logs.ToList();
        }

        public async Task<bool> ClearLogsAsync()
        {
            var logs = await _logRepository.GetAllAsync();
            if (!logs.Any())
                return false;

            _logRepository.RemoveRange(logs);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 
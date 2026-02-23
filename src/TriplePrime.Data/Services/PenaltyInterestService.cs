using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Services
{
    public class PenaltyInterestService : IPenaltyInterestService
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<PenaltyInterestService> _logger;

        private const string DailyRateKey = "PenaltyInterest:DailyRate";
        private const string EnabledKey = "PenaltyInterest:Enabled";
        private const string ConfigGroup = "PenaltyInterest";
        private const decimal DefaultDailyRate = 0.005m; // 0.5% per day

        public PenaltyInterestService(
            IConfigurationService configService,
            ILogger<PenaltyInterestService> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public async Task<decimal> GetDailyInterestRateAsync()
        {
            var value = await _configService.GetConfigurationValueAsync(DailyRateKey);

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Penalty interest daily rate not configured, using default {Rate}%",
                    DefaultDailyRate * 100);
                return DefaultDailyRate;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
            {
                return rate;
            }

            _logger.LogError("Invalid penalty interest rate value '{Value}', using default {Rate}%",
                value, DefaultDailyRate * 100);
            return DefaultDailyRate;
        }

        public async Task SetDailyInterestRateAsync(decimal rate, string userId)
        {
            ValidateRate(rate);

            var value = rate.ToString("F4", CultureInfo.InvariantCulture);
            var description = $"Daily interest rate for overdue payments ({rate * 100:F2}% per day)";

            var config = await _configService.GetConfigurationByKeyAsync(DailyRateKey);
            if (config == null)
            {
                // Create new configuration
                config = new Entities.Configuration
                {
                    Key = DailyRateKey,
                    Value = value,
                    Type = ConfigurationType.Application,
                    Group = ConfigGroup,
                    Description = description,
                    IsEnabled = true,
                    Environment = "Production",
                    ValidFrom = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };
                await _configService.CreateConfigurationAsync(config);
            }
            else
            {
                // Update existing configuration
                config.Value = value;
                config.Description = description;
                config.UpdatedAt = DateTime.UtcNow;
                config.UpdatedBy = userId;
                await _configService.UpdateConfigurationAsync(config);
            }

            _logger.LogInformation(
                "Penalty interest daily rate updated to {Rate}% by {User}",
                rate * 100, userId);
        }

        public async Task<bool> IsPenaltyInterestEnabledAsync()
        {
            var value = await _configService.GetConfigurationValueAsync(EnabledKey);

            if (string.IsNullOrEmpty(value))
            {
                // Default to true if not configured
                return true;
            }

            return bool.TryParse(value, out var enabled) && enabled;
        }

        public async Task SetPenaltyInterestEnabledAsync(bool enabled, string userId)
        {
            var value = enabled.ToString().ToLower();
            var description = $"Master switch to enable/disable penalty interest accrual (currently {(enabled ? "enabled" : "disabled")})";

            var config = await _configService.GetConfigurationByKeyAsync(EnabledKey);
            if (config == null)
            {
                // Create new configuration
                config = new Entities.Configuration
                {
                    Key = EnabledKey,
                    Value = value,
                    Type = ConfigurationType.Application,
                    Group = ConfigGroup,
                    Description = description,
                    IsEnabled = true,
                    Environment = "Production",
                    ValidFrom = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };
                await _configService.CreateConfigurationAsync(config);
            }
            else
            {
                // Update existing configuration
                config.Value = value;
                config.Description = description;
                config.UpdatedAt = DateTime.UtcNow;
                config.UpdatedBy = userId;
                await _configService.UpdateConfigurationAsync(config);
            }

            _logger.LogInformation(
                "Penalty interest accrual {Status} by {User}",
                enabled ? "enabled" : "disabled", userId);
        }

        private static void ValidateRate(decimal rate)
        {
            if (rate < 0.001m || rate > 0.05m)
            {
                throw new ArgumentOutOfRangeException(nameof(rate),
                    "Daily penalty interest rate must be between 0.1% and 5% (0.001 to 0.05 as decimal)");
            }
        }
    }
}

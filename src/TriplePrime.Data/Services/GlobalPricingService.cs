using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Services
{
    public class GlobalPricingService : IGlobalPricingService
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<GlobalPricingService> _logger;

        private const string ConfigKeyPrefix = "GlobalPricingTier:";
        private const string ConfigGroup = "PricingTiers";

        private static readonly Dictionary<int, string> DurationKeyMap = new()
        {
            { 1, "1Month" },
            { 2, "2Month" },
            { 3, "3Month" },
            { 4, "4Month" }
        };

        public GlobalPricingService(
            IConfigurationService configService,
            ILogger<GlobalPricingService> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public async Task<decimal> GetGlobalRateAsync(int durationMonths)
        {
            ValidateDuration(durationMonths);

            var key = GetConfigKey(durationMonths);
            var value = await _configService.GetConfigurationValueAsync(key);

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Global pricing tier not found for duration {Duration} months, defaulting to 0%", durationMonths);
                return 0m;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
            {
                return rate;
            }

            _logger.LogError("Invalid global pricing tier value '{Value}' for duration {Duration} months", value, durationMonths);
            return 0m;
        }

        public async Task<Dictionary<int, decimal>> GetAllGlobalRatesAsync()
        {
            var rates = new Dictionary<int, decimal>();

            foreach (var duration in DurationKeyMap.Keys)
            {
                rates[duration] = await GetGlobalRateAsync(duration);
            }

            return rates;
        }

        public async Task SetGlobalRateAsync(int durationMonths, decimal interestRate, string userId)
        {
            ValidateDuration(durationMonths);
            ValidateRate(interestRate);

            var key = GetConfigKey(durationMonths);
            var description = $"Global interest rate for {durationMonths}-month payment plans";
            var value = interestRate.ToString("F4", CultureInfo.InvariantCulture);

            var config = await _configService.GetConfigurationByKeyAsync(key);
            if (config == null)
            {
                // Create new configuration
                config = new Entities.Configuration
                {
                    Key = key,
                    Value = value,
                    Type = ConfigurationType.Application,
                    Group = ConfigGroup,
                    Description = description,
                    IsEnabled = true,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };
                await _configService.CreateConfigurationAsync(config);
            }
            else
            {
                // Update existing configuration
                config.Value = value;
                config.UpdatedBy = userId;
                await _configService.UpdateConfigurationAsync(config);
            }

            _logger.LogInformation("Global pricing tier set for {Duration} months: {Rate}% by {User}",
                durationMonths, interestRate * 100, userId);
        }

        public async Task SetAllGlobalRatesAsync(Dictionary<int, decimal> rates, string userId)
        {
            foreach (var kvp in rates)
            {
                await SetGlobalRateAsync(kvp.Key, kvp.Value, userId);
            }

            _logger.LogInformation("All global pricing tiers updated by {User}", userId);
        }

        private static string GetConfigKey(int durationMonths)
        {
            return $"{ConfigKeyPrefix}{DurationKeyMap[durationMonths]}";
        }

        private static void ValidateDuration(int durationMonths)
        {
            if (durationMonths < 1 || durationMonths > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(durationMonths),
                    "Duration must be between 1 and 4 months");
            }
        }

        private static void ValidateRate(decimal interestRate)
        {
            if (interestRate < 0 || interestRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(interestRate),
                    "Interest rate must be between 0 and 1 (0% to 100%)");
            }
        }
    }
}

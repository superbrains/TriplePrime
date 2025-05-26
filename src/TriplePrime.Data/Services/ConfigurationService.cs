using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class ConfigurationService
    {
        private readonly IGenericRepository<Configuration> _configRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(
            IGenericRepository<Configuration> configRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<ConfigurationService> logger)
        {
            _configRepository = configRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Configuration> CreateConfigurationAsync(Configuration config)
        {
            config.CreatedAt = DateTime.UtcNow;
            await _configRepository.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Configuration created: {config.Key}");
            return config;
        }

        public async Task<Configuration> GetConfigurationByIdAsync(int id)
        {
            var spec = new ConfigurationSpecification(id);
            return await _configRepository.GetEntityWithSpec(spec);
        }

        public async Task<Configuration> GetConfigurationByKeyAsync(string key, string environment = null)
        {
            var spec = string.IsNullOrEmpty(environment)
                ? new ConfigurationSpecification(key)
                : new ConfigurationSpecification(key, environment);

            var dbConfig = await _configRepository.GetEntityWithSpec(spec);
            if (dbConfig != null) return dbConfig;

            // If not found in database, try to get from appsettings.json
            var appSettingValue = _configuration[key];
            if (string.IsNullOrEmpty(appSettingValue)) return null;

            return new Configuration
            {
                Key = key,
                Value = appSettingValue,
                Type = ConfigurationType.Application,
                IsEnabled = true,
                Environment = environment ?? "Default"
            };
        }

        public async Task<IReadOnlyList<Configuration>> GetConfigurationsByTypeAsync(ConfigurationType type)
        {
            var spec = new ConfigurationSpecification(type);
            spec.ApplyOrderByKey();
            return (await _configRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Configuration>> GetConfigurationsByGroupAsync(string group)
        {
            var spec = new ConfigurationSpecification();
            spec.ApplyGroupFilter(group);
            spec.ApplyOrderByKey();
            return (await _configRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Configuration>> GetConfigurationsByEnvironmentAsync(string environment)
        {
            var spec = new ConfigurationSpecification();
            spec.ApplyEnvironmentFilter(environment);
            spec.ApplyOrderByKey();
            return (await _configRepository.ListAsync(spec)).ToList();
        }

        public async Task<Configuration> UpdateConfigurationAsync(Configuration config)
        {
            var existingConfig = await GetConfigurationByIdAsync(config.Id);
            if (existingConfig == null) return null;

            config.UpdatedAt = DateTime.UtcNow;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Configuration updated: {config.Key}");
            return config;
        }

        public async Task<bool> DeleteConfigurationAsync(int id)
        {
            var config = await GetConfigurationByIdAsync(id);
            if (config == null) return false;

            _configRepository.Remove(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Configuration deleted: {config.Key}");
            return true;
        }

        public async Task<bool> EnableConfigurationAsync(int id)
        {
            var config = await GetConfigurationByIdAsync(id);
            if (config == null) return false;

            config.IsEnabled = true;
            config.UpdatedAt = DateTime.UtcNow;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Configuration enabled: {config.Key}");
            return true;
        }

        public async Task<bool> DisableConfigurationAsync(int id)
        {
            var config = await GetConfigurationByIdAsync(id);
            if (config == null) return false;

            config.IsEnabled = false;
            config.UpdatedAt = DateTime.UtcNow;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Configuration disabled: {config.Key}");
            return true;
        }

        public async Task<IReadOnlyList<Configuration>> SearchConfigurationsAsync(string searchTerm)
        {
            var spec = new ConfigurationSpecification();
            spec.ApplySearchFilter(searchTerm);
            spec.ApplyOrderByKey();
            return (await _configRepository.ListAsync(spec)).ToList();
        }

        public async Task<IReadOnlyList<Configuration>> GetActiveConfigurationsAsync()
        {
            var spec = new ConfigurationSpecification();
            spec.ApplyEnabledFilter(true);
            spec.ApplyValidityFilter(DateTime.UtcNow);
            spec.ApplyOrderByKey();
            return (await _configRepository.ListAsync(spec)).ToList();
        }

        public async Task<Dictionary<string, string>> GetConfigurationDictionaryAsync(string group = null, string environment = null)
        {
            var spec = new ConfigurationSpecification();
            if (!string.IsNullOrEmpty(group)) spec.ApplyGroupFilter(group);
            if (!string.IsNullOrEmpty(environment)) spec.ApplyEnvironmentFilter(environment);
            spec.ApplyEnabledFilter(true);
            spec.ApplyValidityFilter(DateTime.UtcNow);

            var configs = await _configRepository.ListAsync(spec);
            return configs.ToDictionary(c => c.Key, c => c.Value);
        }

        public async Task<bool> SetConfigurationValueAsync(string key, string value, string environment = null)
        {
            var config = await GetConfigurationByKeyAsync(key, environment);
            if (config == null)
            {
                config = new Configuration
                {
                    Key = key,
                    Value = value,
                    Type = ConfigurationType.Application,
                    Environment = environment ?? "Default",
                    IsEnabled = true
                };
                await CreateConfigurationAsync(config);
            }
            else
            {
                config.Value = value;
                config.UpdatedAt = DateTime.UtcNow;
                await UpdateConfigurationAsync(config);
            }

            _logger.LogInformation($"Configuration value set: {key}");
            return true;
        }
    }
} 
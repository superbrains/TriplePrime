using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IConfigurationService
    {
        Task<Configuration> GetConfigurationByKeyAsync(string key);
        Task<IReadOnlyList<Configuration>> GetConfigurationsByGroupAsync(string group);
        Task<IReadOnlyList<Configuration>> GetConfigurationsByEnvironmentAsync(string environment);
        Task<Configuration> SetConfigurationAsync(string key, string value, string description = null);
        Task<bool> DeleteConfigurationAsync(string key);
        Task<bool> ValidateConfigurationAsync(string key, string value);
        Task<string> GetConfigurationValueAsync(string key);
        Task<Dictionary<string, string>> GetAllConfigurationsAsync();
    }
} 
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IConfigurationService
    {
        Task<TriplePrime.Data.Entities.Configuration> GetConfigurationByKeyAsync(string key);
        Task<IReadOnlyList<TriplePrime.Data.Entities.Configuration>> GetConfigurationsByGroupAsync(string group);
        Task<IReadOnlyList<TriplePrime.Data.Entities.Configuration>> GetConfigurationsByEnvironmentAsync(string environment);
        Task<TriplePrime.Data.Entities.Configuration> SetConfigurationAsync(string key, string value, string description = null);
        Task<bool> DeleteConfigurationAsync(string key);
        Task<bool> ValidateConfigurationAsync(string key, string value);
        Task<string> GetConfigurationValueAsync(string key);
        Task<Dictionary<string, string>> GetAllConfigurationsAsync();
    }
} 
using System;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Repositories
{
    public class ConfigurationSpecification : BaseSpecification<Configuration>
    {
        public ConfigurationSpecification(int id)
            : base(c => c.Id == id)
        {
        }

        public ConfigurationSpecification(string key)
            : base(c => c.Key == key)
        {
        }

        public ConfigurationSpecification(ConfigurationType type)
            : base(c => c.Type == type)
        {
        }

        public ConfigurationSpecification(string key, string environment)
            : base(c => c.Key == key && c.Environment == environment)
        {
        }

        public ConfigurationSpecification()
        {
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return;

            Criteria = c => c.Key.Contains(searchTerm) ||
                          c.Value.Contains(searchTerm) ||
                          c.Description.Contains(searchTerm) ||
                          c.Group.Contains(searchTerm) ||
                          c.Environment.Contains(searchTerm);
        }

        public void ApplyTypeFilter(ConfigurationType type)
        {
            Criteria = c => c.Type == type;
        }

        public void ApplyGroupFilter(string group)
        {
            if (string.IsNullOrEmpty(group)) return;
            Criteria = c => c.Group == group;
        }

        public void ApplyEnvironmentFilter(string environment)
        {
            if (string.IsNullOrEmpty(environment)) return;
            Criteria = c => c.Environment == environment;
        }

        public void ApplyEnabledFilter(bool isEnabled)
        {
            Criteria = c => c.IsEnabled == isEnabled;
        }

        public void ApplyEncryptedFilter(bool isEncrypted)
        {
            Criteria = c => c.IsEncrypted == isEncrypted;
        }

        public void ApplyValidityFilter(DateTime date)
        {
            Criteria = c => c.ValidFrom <= date && (!c.ValidTo.HasValue || c.ValidTo >= date);
        }

        public void ApplyVersionFilter(string version)
        {
            if (string.IsNullOrEmpty(version)) return;
            Criteria = c => c.Version == version;
        }

        public void ApplyOrderByKey(bool descending = false)
        {
            if (descending)
                ApplyOrderByDescending(c => c.Key);
            else
                ApplyOrderBy(c => c.Key);
        }

        public void ApplyOrderByCreatedDate(bool descending = true)
        {
            if (descending)
                ApplyOrderByDescending(c => c.CreatedAt);
            else
                ApplyOrderBy(c => c.CreatedAt);
        }
    }
} 
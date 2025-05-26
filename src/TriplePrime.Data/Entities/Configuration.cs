using System;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Entities
{
    public enum ConfigurationType
    {
        System,
        Application,
        User,
        Feature,
        Integration
    }

    public class Configuration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public ConfigurationType Type { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsEncrypted { get; set; }

        public bool IsEnabled { get; set; } = true;

        [MaxLength(100)]
        public string Group { get; set; }

        [MaxLength(100)]
        public string Environment { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        public DateTime? ValidTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        [MaxLength(100)]
        public string Version { get; set; }
    }
} 
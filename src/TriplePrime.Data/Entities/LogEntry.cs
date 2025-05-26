using System;
using System.ComponentModel.DataAnnotations;

namespace TriplePrime.Data.Entities
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public LogLevel Level { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        [MaxLength(100)]
        public string Source { get; set; }

        [MaxLength(2000)]
        public string Exception { get; set; }

        [MaxLength(4000)]
        public string StackTrace { get; set; }

        [MaxLength(100)]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        [MaxLength(100)]
        public string UserName { get; set; }

        [MaxLength(50)]
        public string RequestId { get; set; }

        [MaxLength(500)]
        public string RequestPath { get; set; }

        [MaxLength(10)]
        public string RequestMethod { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; }
    }
} 
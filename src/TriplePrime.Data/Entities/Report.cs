using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public enum ReportType
    {
        Sales,
        Marketing,
        Financial,
        Delivery,
        User,
        Custom
    }

    public enum ReportStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public ReportType Type { get; set; }

        [Required]
        public ReportStatus Status { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string FileName { get; set; }

        [MaxLength(500)]
        public string FilePath { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Parameters { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
} 
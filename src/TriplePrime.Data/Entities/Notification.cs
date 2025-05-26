using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriplePrime.Data.Entities
{
    public enum NotificationType
    {
        System,
        Email,
        SMS,
        Push
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed,
        Read
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationStatus Status { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        [MaxLength(500)]
        public string RecipientEmail { get; set; }

        [MaxLength(20)]
        public string RecipientPhone { get; set; }

        [MaxLength(500)]
        public string ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
} 
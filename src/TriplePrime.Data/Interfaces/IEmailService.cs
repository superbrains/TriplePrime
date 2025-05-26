using System.Threading.Tasks;
using TriplePrime.Data.Models;

namespace TriplePrime.Data.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath);
        Task SendBulkEmailAsync(string[] recipients, string subject, string body);
        Task<bool> ValidateEmailAsync(string email);
        Task<bool> UpdateEmailSettingsAsync(EmailSettings settings);
        Task<EmailSettings> GetEmailSettingsAsync();
    }
} 
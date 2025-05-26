using System.Threading.Tasks;
using TriplePrime.Data.Models;

namespace TriplePrime.Data.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string to, string message);
        Task SendBulkSmsAsync(string[] recipients, string message);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
        Task<bool> UpdateSmsSettingsAsync(SmsSettings settings);
        Task<SmsSettings> GetSmsSettingsAsync();
    }
} 
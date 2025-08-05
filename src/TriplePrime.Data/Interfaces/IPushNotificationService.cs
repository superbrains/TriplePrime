using System.Collections.Generic;
using System.Threading.Tasks;

namespace TriplePrime.Data.Interfaces
{
    public interface IPushNotificationService
    {
        Task<bool> SendNotificationAsync(List<string> recipientTokens, string title, string body, Dictionary<string, string> data = null);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<IReadOnlyList<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, string firstName, string lastName, string phoneNumber);
        Task<bool> UpdateUserAddressAsync(string userId, string address, string city, string state, string country, string postalCode);
        Task<bool> UpdateNotificationPreferencesAsync(string userId, bool emailNotifications, bool smsNotifications, bool pushNotifications);
        Task<bool> UpdateDeliveryPreferencesAsync(string userId, string preferredDeliveryTime, string preferredDeliveryDay);
        Task<bool> AddDeliveryAddressAsync(string userId, DeliveryAddress address);
        Task<bool> RemoveDeliveryAddressAsync(string userId, int addressId);
        Task<IReadOnlyList<DeliveryAddress>> GetUserDeliveryAddressesAsync(string userId);
        Task<bool> UpdateDeviceTokenAsync(string userId, string deviceToken);
    }
} 
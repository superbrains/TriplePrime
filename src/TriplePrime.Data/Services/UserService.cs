using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user: {string.Join(", ", result.Errors)}");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return user;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var spec = new UserSpecification(userId);
            return await _unitOfWork.Repository<ApplicationUser>().GetEntityWithSpec(spec);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            var spec = new UserSpecification();
            spec.ApplyEmailFilter(email);
            return await _unitOfWork.Repository<ApplicationUser>().FirstOrDefaultAsync(spec);
        }

        public async Task UpdateUserProfileAsync(string userId, UserProfileModel profile)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.PhoneNumber = profile.PhoneNumber;
            user.Address = profile.Address;

            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            user.LanguagePreference = preferences.LanguagePreference;

            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string roleName)
        {
            var spec = new UserSpecification();
            spec.ApplyRoleFilter(roleName);
            return await _unitOfWork.Repository<ApplicationUser>().ListAsync(spec);
        }

        public async Task<bool> IsUserActiveAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user != null && user.IsActive;
        }

        public async Task DeactivateUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivateUserAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            var spec = new UserSpecification();
            spec.ApplySearchFilter(searchTerm);
            return await _unitOfWork.Repository<ApplicationUser>().ListAsync(spec);
        }

        public async Task<bool> UpdateDeviceTokenAsync(string userId, string deviceToken)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.DeviceToken = deviceToken;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}


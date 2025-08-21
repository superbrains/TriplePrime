using System;
using System.Collections.Generic;
using System.Linq;
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
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Profile data cannot be null");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Check if email is being changed
            if (!string.IsNullOrEmpty(profile.Email) && user.Email != profile.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(profile.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    throw new ArgumentException("Email address is already in use");
                }
                
                user.Email = profile.Email;
                user.UserName = profile.Email;  // Since we use email as username
                user.NormalizedEmail = profile.Email.ToUpperInvariant();
                user.NormalizedUserName = profile.Email.ToUpperInvariant();
            }

            // Update profile fields
            if (!string.IsNullOrWhiteSpace(profile.FirstName))
            {
                user.FirstName = profile.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(profile.LastName))
            {
                user.LastName = profile.LastName.Trim();
            }

            if (profile.PhoneNumber != null)
            {
                user.PhoneNumber = profile.PhoneNumber.Trim();
            }

            if (profile.Address != null)
            {
                user.Address = profile.Address.Trim();
            }

            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user profile: {errors}");
            }
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

        public async Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ChangePasswordResult 
                { 
                    Success = false, 
                    Error = "User ID is required" 
                };
            }

            if (request == null)
            {
                return new ChangePasswordResult 
                { 
                    Success = false, 
                    Error = "Password change request data is required" 
                };
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ChangePasswordResult 
                    { 
                        Success = false, 
                        Error = "User not found" 
                    };
                }

                // Verify current password
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!passwordValid)
                {
                    return new ChangePasswordResult 
                    { 
                        Success = false, 
                        Error = "Current password is incorrect" 
                    };
                }

                // Check if new password is same as current
                if (request.CurrentPassword == request.NewPassword)
                {
                    return new ChangePasswordResult 
                    { 
                        Success = false, 
                        Error = "New password must be different from current password" 
                    };
                }

                // Change the password
                var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!changeResult.Succeeded)
                {
                    var errors = changeResult.Errors.Select(e => GetUserFriendlyPasswordError(e.Code, e.Description));
                    return new ChangePasswordResult 
                    { 
                        Success = false, 
                        Error = string.Join(" ", errors) 
                    };
                }

                user.UpdatedAt = DateTime.UtcNow;

                return new ChangePasswordResult 
                { 
                    Success = true, 
                    Message = "Password changed successfully" 
                };
            }
            catch (Exception ex)
            {
                return new ChangePasswordResult 
                { 
                    Success = false, 
                    Error = "An error occurred while changing the password. Please try again later." 
                };
            }
        }

        private string GetUserFriendlyPasswordError(string code, string description)
        {
            switch (code)
            {
                case "PasswordTooShort":
                    return "Password must be at least 8 characters long.";
                case "PasswordRequiresNonAlphanumeric":
                    return "Password must contain at least one special character.";
                case "PasswordRequiresDigit":
                    return "Password must contain at least one number.";
                case "PasswordRequiresUpper":
                    return "Password must contain at least one uppercase letter.";
                case "PasswordRequiresLower":
                    return "Password must contain at least one lowercase letter.";
                case "PasswordRequiresUniqueChars":
                    return "Password must contain more unique characters.";
                case "PasswordMismatch":
                    return "Current password is incorrect.";
                default:
                    return description ?? "Password does not meet security requirements.";
            }
        }
    }

    public class ChangePasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}


using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TriplePrime.Data.Services
{
    public class AuthenticationService 
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid email or password" };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Account is deactivated" };
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid email or password" };
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthenticationResult
            {
                Success = true,
                User = user,
                Claims = claims,
                Roles = roles
            };
        }

        public async Task<AuthenticationResult> RegisterAsync(ApplicationUser user, string password)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = string.Join(", ", result.Errors)
                    };
                }

                // Add default claims
                await _userManager.AddClaimAsync(user, new Claim("user_type", "customer"));
                await _userManager.AddToRoleAsync(user, "Customer");

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new AuthenticationResult
                {
                    Success = true,
                    User = user
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AuthenticationResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "User not found" };
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", result.Errors)
                };
            }

            return new AuthenticationResult { Success = true };
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> ValidateTokenAsync(string userId, string token, string purpose)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, purpose, token);
        }

        public async Task<AuthenticationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "User not found" };
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", result.Errors)
                };
            }

            return new AuthenticationResult { Success = true };
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }

        public async Task<bool> IsLockedOutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsLockedOutAsync(user);
        }

        public async Task<IEnumerable<UserDetails>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u => u.UserRoles)
                    .ToListAsync();

                var userDetails = new List<UserDetails>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var isLocked = await _userManager.IsLockedOutAsync(user);

                    userDetails.Add(new UserDetails
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "Customer",
                        Status = !user.IsActive ? "Inactive" : isLocked ? "Locked" : "Active",
                        RegistrationDate = user.CreatedAt
                    });
                }

                return userDetails;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch users", ex);
            }
        }

        public async Task<UserDetails> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Update basic information
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.IsActive = request.Status.ToLower() == "active";

            // Update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.FirstOrDefault() != request.Role)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, request.Role);
            }

            // Update lock status
            if (request.Status.ToLower() == "locked")
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else if (request.Status.ToLower() == "active")
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to update user: {string.Join(", ", result.Errors)}");
            }

            // Return updated user details
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);

            return new UserDetails
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Customer",
                Status = !user.IsActive ? "Inactive" : isLocked ? "Locked" : "Active",
                RegistrationDate = user.CreatedAt
            };
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to delete user: {string.Join(", ", result.Errors)}");
            }
        }

        public async Task<UserDetails> ChangeUserStatusAsync(string userId, string status)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            switch (status.ToLower())
            {
                case "active":
                    user.IsActive = true;
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    break;
                case "inactive":
                    user.IsActive = false;
                    break;
                case "locked":
                    user.IsActive = true;
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    break;
                default:
                    throw new ArgumentException("Invalid status");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to update user status: {string.Join(", ", result.Errors)}");
            }

            // Return updated user details
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);

            return new UserDetails
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Customer",
                Status = !user.IsActive ? "Inactive" : isLocked ? "Locked" : "Active",
                RegistrationDate = user.CreatedAt
            };
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ApplicationUser User { get; set; }
        public IList<Claim> Claims { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class UserDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
} 
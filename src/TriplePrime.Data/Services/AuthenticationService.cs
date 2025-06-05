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
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string email, string password);
        Task<AuthenticationResult> RegisterAsync(ApplicationUser user, string password, string referralCode = null);
        Task LogoutAsync();
        Task<AuthenticationResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ValidateTokenAsync(string userId, string token, string purpose);
        Task<AuthenticationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ValidateCredentialsAsync(string email, string password);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> LockUserAsync(string userId);
        Task<bool> UnlockUserAsync(string userId);
        Task<bool> IsLockedOutAsync(string userId);
        Task<IEnumerable<UserDetails>> GetAllUsersAsync();
        Task<UserDetails> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task DeleteUserAsync(string userId);
        Task<UserDetails> ChangeUserStatusAsync(string userId, string status);
        Task<ServiceResponse> ForgotPasswordAsync(string email);
        Task<ServiceResponse> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            try
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

                // Generate JWT token
                var token = GenerateJwtToken(user, roles);

                // Add token to claims
                claims.Add(new Claim("token", token));

                return new AuthenticationResult
                {
                    Success = true,
                    User = user,
                    Claims = claims,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                // Log the error here if you have a logging service
                return new AuthenticationResult 
                { 
                    Success = false, 
                    ErrorMessage = "An error occurred during login. Please try again later." 
                };
            }
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthenticationResult> RegisterAsync(ApplicationUser user, string password, string referralCode = null)
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

                // Handle referral if referral code is provided
                if (!string.IsNullOrEmpty(referralCode))
                {
                    var marketerSpec = new MarketerSpecification(referralCode);
                    var marketer = await _unitOfWork.Repository<Marketer>().GetEntityWithSpec(marketerSpec);
                    
                    if (marketer != null)
                    {
                        var referral = new Referral
                        {
                            MarketerId = marketer.UserId,
                            ReferredUserId = user.Id,
                            ReferralCode = referralCode,
                            Status = ReferralStatus.Pending,
                            CommissionAmount = 0, // Will be calculated when the customer makes a purchase
                            CommissionPaid = false,
                            ReferralDate = DateTime.UtcNow
                        };

                        await _unitOfWork.Repository<Referral>().AddAsync(referral);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Send welcome email based on user role
                try
                {
                    var baseUrl = _configuration["AppSettings:ApiBaseUrl"];
                    var loginUrl = $"{baseUrl}/login";

                    var emailModel = new
                    {
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Email = user.Email,
                        Password = password, // Only for initial welcome email
                        LoginUrl = loginUrl
                    };

                    // Determine which template to use based on user role
                    var roles = await _userManager.GetRolesAsync(user);
                    string templateName;
                    string subject;

                    if (roles.Contains("Admin") || roles.Contains("Marketer"))
                    {
                        templateName = "AdminWelcomeTemplate.html";
                        subject = "Welcome to TriplePrime - Staff Account Created";
                        var staffModel = new
                        {
                            Name = emailModel.Name,
                            Email = emailModel.Email,
                            Password = emailModel.Password,
                            LoginUrl = emailModel.LoginUrl,
                            Role = roles.First() // "Admin" or "Marketer"
                        };
                        await _emailService.SendTemplatedEmailAsync(user.Email, subject, templateName, staffModel);
                    }
                    else
                    {
                        templateName = "CustomerWelcomeTemplate.html";
                        subject = "Welcome to TriplePrime!";
                        await _emailService.SendTemplatedEmailAsync(user.Email, subject, templateName, emailModel);
                    }

                    _logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the registration
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                }

                return new AuthenticationResult
                {
                    Success = true,
                    User = user
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Registration failed for {Email}", user.Email);
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
                        PhoneNumber = user.PhoneNumber,
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
                PhoneNumber = user.PhoneNumber,
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
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "Customer",
                Status = !user.IsActive ? "Inactive" : isLocked ? "Locked" : "Active",
                RegistrationDate = user.CreatedAt
            };
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Return success even if user doesn't exist to prevent email enumeration
                    return new ServiceResponse { Success = true, Message = "If your email is registered, you will receive password reset instructions." };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

                var emailBody = $@"
                    <h2>Password Reset Request</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>We received a request to reset your password. Click the link below to reset your password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>If you didn't request this, you can safely ignore this email.</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>Best regards,<br>TriplePrime Team</p>";

                await _emailService.SendEmailAsync(
                    email,
                    "Reset Your Password - TriplePrime",
                    emailBody
                );

                return new ServiceResponse { Success = true, Message = "Password reset instructions have been sent to your email." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync for email: {Email}", email);
                return new ServiceResponse { Success = false, Message = "An error occurred while processing your request." };
            }
        }

        public async Task<ServiceResponse> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ServiceResponse { Success = false, Message = "Invalid request." };
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    return new ServiceResponse { Success = true, Message = "Your password has been reset successfully." };
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ServiceResponse { Success = false, Message = errors };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordWithTokenAsync for email: {Email}", email);
                return new ServiceResponse { Success = false, Message = "An error occurred while resetting your password." };
            }
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
        public string PhoneNumber { get; set; }
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
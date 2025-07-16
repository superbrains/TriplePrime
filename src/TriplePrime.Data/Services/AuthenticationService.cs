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
using TriplePrime.Data.Specifications;

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
                // Validate user data
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Email address is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Both first name and last name are required."
                    };
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "An account with this email address already exists."
                    };
                }

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errorMessage = "Registration failed: ";
                    foreach (var error in result.Errors)
                    {
                        switch (error.Code)
                        {
                            case "PasswordTooShort":
                                errorMessage = "Password must be at least 6 characters long.";
                                break;
                            case "PasswordRequiresNonAlphanumeric":
                                errorMessage = "Password must contain at least one special character.";
                                break;
                            case "PasswordRequiresDigit":
                                errorMessage = "Password must contain at least one number.";
                                break;
                            case "PasswordRequiresUpper":
                                errorMessage = "Password must contain at least one uppercase letter.";
                                break;
                            case "PasswordRequiresLower":
                                errorMessage = "Password must contain at least one lowercase letter.";
                                break;
                            case "DuplicateUserName":
                                errorMessage = "This username is already taken.";
                                break;
                            case "InvalidUserName":
                                errorMessage = "Invalid username. Use only letters and numbers.";
                                break;
                            case "InvalidEmail":
                                errorMessage = "Please enter a valid email address.";
                                break;
                            default:
                                errorMessage = error.Description;
                                break;
                        }
                        // Break after first error to avoid overwhelming the user
                        break;
                    }

                    _logger.LogWarning("User registration failed for {Email}: {Error}", 
                        user.Email, errorMessage);

                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = errorMessage
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
                    .OrderByDescending(u => u.CreatedAt)
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

            const int batchSize = 100;
            try
            {
                // 1. Delete all payment schedules first (they depend on savings plans)
                var scheduleSpec = new PaymentScheduleSpecification();
                scheduleSpec.ApplyUserFilter(userId);
                var schedules = await _unitOfWork.Repository<PaymentSchedule>().ListAsync(scheduleSpec);
                
                foreach (var batch in schedules.Chunk(batchSize))
                {
                    foreach (var schedule in batch)
                    {
                        _unitOfWork.Repository<PaymentSchedule>().Remove(schedule);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 2. Delete all savings plans
                var plansSpec = new SavingsPlanSpecification();
                plansSpec.ApplyUserFilter(userId);
                var plans = await _unitOfWork.Repository<SavingsPlan>().ListAsync(plansSpec);
                
                foreach (var batch in plans.Chunk(batchSize))
                {
                    foreach (var plan in batch)
                    {
                        _unitOfWork.Repository<SavingsPlan>().Remove(plan);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 3. Delete all commissions first (they depend on referrals)
                var marketerSpec = new MarketerSpecification(userId);
                var marketer = await _unitOfWork.Repository<Marketer>().GetEntityWithSpec(marketerSpec);
                if (marketer != null)
                {
                    var commissionSpec = new CommissionSpecification();
                    commissionSpec.ApplyMarketerFilter(marketer.Id);
                    var commissions = await _unitOfWork.Repository<Commission>().ListAsync(commissionSpec);
                    
                    foreach (var batch in commissions.Chunk(batchSize))
                    {
                        foreach (var commission in batch)
                        {
                            _unitOfWork.Repository<Commission>().Remove(commission);
                        }
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // 4. Delete all referrals
                var referralsAsReferrerSpec = new ReferralSpecification(userId, true);
                var referralsAsReferredSpec = new ReferralSpecification(userId, true, true);
                
                var allReferrals = new List<Referral>();
                allReferrals.AddRange(await _unitOfWork.Repository<Referral>().ListAsync(referralsAsReferrerSpec));
                allReferrals.AddRange(await _unitOfWork.Repository<Referral>().ListAsync(referralsAsReferredSpec));
                
                foreach (var batch in allReferrals.Chunk(batchSize))
                {
                    foreach (var referral in batch)
                    {
                        _unitOfWork.Repository<Referral>().Remove(referral);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 5. Delete all payments
                var paymentSpec = new PaymentSpecification();
                paymentSpec.ApplyUserFilter(userId);
                var payments = await _unitOfWork.Repository<Payment>().ListAsync(paymentSpec);

                foreach (var batch in payments.Chunk(batchSize))
                {
                    foreach (var payment in batch)
                    {
                        _unitOfWork.Repository<Payment>().Remove(payment);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 6. Delete all food pack purchases
                var purchaseSpec = new FoodPackPurchaseSpecification();
                purchaseSpec.ApplyUserFilter(userId);
                var purchases = await _unitOfWork.Repository<FoodPackPurchase>().ListAsync(purchaseSpec);

                foreach (var batch in purchases.Chunk(batchSize))
                {
                    foreach (var purchase in batch)
                    {
                        _unitOfWork.Repository<FoodPackPurchase>().Remove(purchase);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 7. Delete all deliveries
                var deliverySpec = new DeliverySpecification();
                deliverySpec.ApplyUserFilter(userId);
                var deliveries = await _unitOfWork.Repository<Delivery>().ListAsync(deliverySpec);

                foreach (var batch in deliveries.Chunk(batchSize))
                {
                    foreach (var delivery in batch)
                    {
                        _unitOfWork.Repository<Delivery>().Remove(delivery);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 8. Delete all reports
                var reportSpec = new ReportSpecification(userId);
                var reports = await _unitOfWork.Repository<Report>().ListAsync(reportSpec);

                foreach (var batch in reports.Chunk(batchSize))
                {
                    foreach (var report in batch)
                    {
                        _unitOfWork.Repository<Report>().Remove(report);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 9. Delete the marketer record if exists
                if (marketer != null)
                {
                    _unitOfWork.Repository<Marketer>().Remove(marketer);
                    await _unitOfWork.SaveChangesAsync();
                }

                // 10. Delete all payment methods
                var paymentMethodSpec = new PaymentMethodSpecification();
                paymentMethodSpec.ApplyUserFilter(userId);
                var paymentMethods = await _unitOfWork.Repository<PaymentMethod>().ListAsync(paymentMethodSpec);
                
                foreach (var batch in paymentMethods.Chunk(batchSize))
                {
                    foreach (var method in batch)
                    {
                        _unitOfWork.Repository<PaymentMethod>().Remove(method);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 11. Remove user roles and claims
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                }
                var userClaims = await _userManager.GetClaimsAsync(user);
                if (userClaims.Any())
                {
                    await _userManager.RemoveClaimsAsync(user, userClaims);
                }

                // 12. Finally delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to delete user: {errors}");
                }

                _logger.LogInformation("Successfully deleted user {UserId} and all associated data", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", userId);
                throw new InvalidOperationException("Failed to delete user. Please try again or contact support if the issue persists.", ex);
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
                var resetLink = $"https://tripleprime.com.ng/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

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
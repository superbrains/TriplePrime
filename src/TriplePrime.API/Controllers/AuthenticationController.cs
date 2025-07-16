using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TriplePrime.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly AuthenticationService _authService;
        private readonly UserService _userService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(AuthenticationService authService, UserService userService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        private string GetUserFriendlyErrorMessage(string error)
        {
            // Map common ASP.NET Identity error messages to user-friendly messages
            if (error.Contains("DuplicateUserName") || error.Contains("DuplicateEmail"))
                return "An account with this email already exists.";
            
            if (error.Contains("PasswordTooShort"))
                return "Password must be at least 8 characters long.";
            
            if (error.Contains("PasswordRequiresNonAlphanumeric"))
                return "Password must contain at least one special character.";
            
            if (error.Contains("PasswordRequiresDigit"))
                return "Password must contain at least one number.";
            
            if (error.Contains("PasswordRequiresUpper"))
                return "Password must contain at least one uppercase letter.";
            
            if (error.Contains("PasswordRequiresLower"))
                return "Password must contain at least one lowercase letter.";
            
            if (error.Contains("InvalidEmail"))
                return "Please enter a valid email address.";
            
            if (error.Contains("UserNotFound"))
                return "Account not found. Please check your credentials.";
            
            if (error.Contains("PasswordMismatch"))
                return "Incorrect password. Please try again.";

            // Default message for unknown errors
            return "An error occurred. Please try again or contact support if the issue persists.";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Email, request.Password);
                if (!result.Success)
                {
                    return HandleResponse(ApiResponse.ErrorResponse(GetUserFriendlyErrorMessage(result.ErrorMessage)));
                }
                return HandleResponse(ApiResponse<AuthenticationResult>.SuccessResponse(result));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Email}", request.Email);
                return HandleResponse(ApiResponse.ErrorResponse("Unable to process login request. Please try again later."));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    IsActive = true
                };

                string password = request.Password;
                if (string.IsNullOrEmpty(password))
                {
                    password = GenerateRandomPassword();
                }

                var result = await _authService.RegisterAsync(user, password, request.ReferralCode);
                if (!result.Success)
                {
                    var errorMessages = result.ErrorMessage.Split(',')
                        .Select(error => GetUserFriendlyErrorMessage(error.Trim()))
                        .Distinct()
                        .ToList();

                    return HandleResponse(ApiResponse.ErrorResponse(string.Join(" ", errorMessages)));
                }

                var response = new
                {
                    result.Success,
                    result.User,
                    GeneratedPassword = string.IsNullOrEmpty(request.Password) ? password : null
                };

                return HandleResponse(ApiResponse<object>.SuccessResponse(response));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return HandleResponse(ApiResponse.ErrorResponse("Unable to complete registration. Please try again later."));
            }
        }

        private string GenerateRandomPassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}|;:,.<>?";
            const int length = 12; // Password length

            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);

                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = validChars[bytes[i] % validChars.Length];
                }

                return new string(chars);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return HandleResponse(ApiResponse.SuccessResponse("Logged out successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var response = await _authService.ForgotPasswordAsync(request.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var response = await _authService.ResetPasswordWithTokenAsync(request.Email, request.Token, request.NewPassword);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPassword for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while resetting your password." });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                if (!result.Success)
                {
                    return HandleResponse(ApiResponse.ErrorResponse(result.ErrorMessage));
                }
                return HandleResponse(ApiResponse.SuccessResponse("Password changed successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken([FromQuery] string userId, [FromQuery] string token, [FromQuery] string purpose)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(userId, token, purpose);
                return HandleResponse(ApiResponse<bool>.SuccessResponse(isValid));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return HandleResponse(ApiResponse<IEnumerable<UserDetails>>.SuccessResponse(users));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var updatedUser = await _authService.UpdateUserAsync(userId, request);
                return HandleResponse(ApiResponse<UserDetails>.SuccessResponse(updatedUser));
            }
            catch (ArgumentException ex)
            {
                return HandleResponse(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                await _authService.DeleteUserAsync(userId);
                return HandleResponse(ApiResponse.SuccessResponse("User has been successfully deleted."));
            }
            catch (ArgumentException ex)
            {
                return HandleResponse(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", userId);
                return HandleResponse(ApiResponse.ErrorResponse("Unable to delete user. Please ensure there are no active plans or pending transactions."));
            }
        }

        [HttpPatch("users/{userId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserStatus(string userId, [FromBody] ChangeStatusRequest request)
        {
            try
            {
                var updatedUser = await _authService.ChangeUserStatusAsync(userId, request.Status);
                return HandleResponse(ApiResponse<UserDetails>.SuccessResponse(updatedUser));
            }
            catch (ArgumentException ex)
            {
                return HandleResponse(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to change status for user {UserId} to {Status}", userId, request.Status);
                return HandleResponse(ApiResponse.ErrorResponse("Unable to update user status. Please try again later."));
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ReferralCode { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangeStatusRequest
    {
        public string Status { get; set; }
    }
} 
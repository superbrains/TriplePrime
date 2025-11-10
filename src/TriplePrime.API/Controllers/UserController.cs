using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;
using TriplePrime.API.Models;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return HandleResponse(ApiResponse<ApplicationUser>.ErrorResponse("User not found"));
                }
                return HandleResponse(ApiResponse<ApplicationUser>.SuccessResponse(user));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return HandleResponse(ApiResponse<ApplicationUser>.ErrorResponse("User not found"));
                }
                return HandleResponse(ApiResponse<ApplicationUser>.SuccessResponse(user));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateUserProfile(string id, [FromBody] UserProfileModel profile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(ApiResponse.ErrorResponse("User ID is required"));
                }

                if (profile == null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Profile data is required"));
                }

                if (string.IsNullOrWhiteSpace(profile.FirstName))
                {
                    return BadRequest(ApiResponse.ErrorResponse("First name is required"));
                }

                if (string.IsNullOrWhiteSpace(profile.LastName))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Last name is required"));
                }

                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    if (!IsValidEmail(profile.Email))
                    {
                        return BadRequest(ApiResponse.ErrorResponse("Please provide a valid email address"));
                    }
                }

                if (!string.IsNullOrWhiteSpace(profile.PhoneNumber))
                {
                    if (!IsValidPhoneNumber(profile.PhoneNumber))
                    {
                        return BadRequest(ApiResponse.ErrorResponse("Please provide a valid phone number"));
                    }
                }

                await _userService.UpdateUserProfileAsync(id, profile);
                return Ok(ApiResponse.SuccessResponse("User profile updated successfully"));
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ApiResponse.ErrorResponse("User not found"));
                }
                if (ex.Message.Contains("already in use"))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Email address is already in use by another account"));
                }
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                var errorMessage = "An error occurred while updating the profile. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Profile update error: {ex.Message}");
                return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            
            var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
        }

        [HttpPut("preferences/{id}")]
        public async Task<IActionResult> UpdateUserPreferences(string id, [FromBody] UserPreferences preferences)
        {
            try
            {
                await _userService.UpdateUserPreferencesAsync(id, preferences);
                return HandleResponse(ApiResponse.SuccessResponse("User preferences updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleName);
                return HandleResponse(ApiResponse<IEnumerable<ApplicationUser>>.SuccessResponse(users));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("active/{id}")]
        public async Task<IActionResult> IsUserActive(string id)
        {
            try
            {
                var isActive = await _userService.IsUserActiveAsync(id);
                return HandleResponse(ApiResponse<bool>.SuccessResponse(isActive));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            try
            {
                await _userService.DeactivateUserAsync(id);
                return HandleResponse(ApiResponse.SuccessResponse("User deactivated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            try
            {
                await _userService.ActivateUserAsync(id);
                return HandleResponse(ApiResponse.SuccessResponse("User activated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
        {
            try
            {
                var users = await _userService.SearchUsersAsync(searchTerm);
                return HandleResponse(ApiResponse<IEnumerable<ApplicationUser>>.SuccessResponse(users));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("device-token/{id}")]
        public async Task<IActionResult> UpdateDeviceToken(string id, [FromBody] UpdateDeviceTokenRequest request)
        {
            try
            {
                await _userService.UpdateDeviceTokenAsync(id, request.DeviceToken);
                return HandleResponse(ApiResponse.SuccessResponse("Device token updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] TriplePrime.Data.Models.ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(ApiResponse.ErrorResponse("User ID is required"));
                }

                if (request == null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Password change request data is required"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse.ErrorResponse(string.Join(" ", errors)));
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(ApiResponse.ErrorResponse("New password and confirmation password do not match"));
                }

                var result = await _userService.ChangePasswordAsync(id, request);
                
                if (!result.Success)
                {
                    if (result.Error.Contains("not found"))
                    {
                        return NotFound(ApiResponse.ErrorResponse(result.Error));
                    }
                    
                    if (result.Error.Contains("incorrect") || result.Error.Contains("different"))
                    {
                        return BadRequest(ApiResponse.ErrorResponse(result.Error));
                    }
                    
                    return BadRequest(ApiResponse.ErrorResponse(result.Error));
                }

                return Ok(ApiResponse.SuccessResponse(result.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                var errorMessage = "An error occurred while changing the password. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Password change error: {ex.Message}");
                return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(ApiResponse.ErrorResponse("User ID is required"));
                }

                var result = await _userService.DeleteUserAsync(id);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(ApiResponse.ErrorResponse(result.Message));
                    }
                    
                    if (result.Message.Contains("ongoing savings plans"))
                    {
                        return BadRequest(ApiResponse.ErrorResponse(result.Message));
                    }
                    
                    return BadRequest(ApiResponse.ErrorResponse(result.Message));
                }

                return Ok(ApiResponse.SuccessResponse(result.Message));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 
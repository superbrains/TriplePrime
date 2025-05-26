using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

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
                await _userService.UpdateUserProfileAsync(id, profile);
                return HandleResponse(ApiResponse.SuccessResponse("User profile updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
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
    }
} 
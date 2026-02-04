using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Profile;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// User Profile Management
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AuthService _authService;

        public ProfileController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Get current user's ID from JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="request">Profile update details (all fields are optional)</param>
        /// <response code="200">Profile updated successfully</response>
        /// <response code="400">Invalid data or business rule violation</response>
        /// <response code="401">Not authenticated or invalid token</response>
        /// <response code="404">User not found</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                // Extract userId from JWT token
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Update profile
                var updatedUser = await _authService.UpdateProfile(
                    userId,
                    request.DisplayName,
                    request.Email,
                    request.PhoneNumber
                );

                if (updatedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get role name for response
                var role = await _authService.GetRoleById(updatedUser.RoleId);

                // Return updated profile (exclude password)
                return Ok(new
                {
                    message = "Profile updated successfully",
                    user = new
                    {
                        userId = updatedUser.UserId,
                        username = updatedUser.Username,
                        displayName = updatedUser.DisplayName,
                        email = updatedUser.Email,
                        phoneNumber = updatedUser.PhoneNumber,
                        roleId = updatedUser.RoleId,
                        roleName = role?.RoleName ?? "Unknown",
                        isActive = updatedUser.IsActive,
                        createdAt = updatedUser.CreatedAt
                    }
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Check for unique constraint violations
                if (dbEx.InnerException?.Message.Contains("Users_email_key") == true)
                {
                    return BadRequest(new { message = "Email already exists" });
                }
                if (dbEx.InnerException?.Message.Contains("Users_phoneNumber_key") == true)
                {
                    return BadRequest(new { message = "Phone number already exists" });
                }
                return BadRequest(new { message = "Database error occurred", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Handle business rule violations
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

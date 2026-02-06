using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// Authentication
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <response code="200">Login successful, returns user info and JWT token</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Invalid credentials or account is inactive</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            var (response, errorMessage) = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = errorMessage });
            }

            return Ok(response);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Old and new passwords</param>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid data or incorrect old password</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="404">User not found</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var (user, errorMessage) = await _authService.ChangePasswordAsync(
                userId,
                request.OldPassword,
                request.NewPassword
            );

            if (user == null)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new 
            { 
                message = "Password changed successfully. You can now access all features.",
                userId = user.UserId,
                username = user.Username
            });
        }

        // ❌ NO REGISTER ENDPOINT
        // Only Admin can create user accounts via /api/users endpoint
    }
}

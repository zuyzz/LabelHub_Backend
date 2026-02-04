using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// User Management
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly AuthService _authService;

        public UsersController(AuthService authService)
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
        /// Get all users
        /// </summary>
        /// <response code="200">List of users</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _authService.GetAllUsers();
            var roles = await _authService.GetAllRoles();

            // Map to UserResponse DTOs
            var response = users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = roles.FirstOrDefault(r => r.RoleId == u.RoleId)?.RoleName ?? "Unknown",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .OrderBy(u => u.Username)
            .ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">User details</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _authService.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var role = await _authService.GetRoleById(user.RoleId);

            var response = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = role?.RoleName ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="request">User details</param>
        /// <response code="201">User created</response>
        /// <response code="400">Invalid data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                // Verify role exists
                var role = await _authService.GetRoleById(request.RoleId);
                if (role == null)
                {
                    return BadRequest(new { message = "Invalid role specified" });
                }

                var user = await _authService.CreateUser(
                    request.Username,
                    request.DisplayName,
                    request.Email,
                    request.PhoneNumber,
                    request.RoleId
                );

                var response = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    RoleName = role.RoleName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = user.UserId }, response);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Check for unique constraint violations
                if (dbEx.InnerException?.Message.Contains("Users_username_key") == true)
                {
                    return BadRequest(new { message = "Username already exists" });
                }
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
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Update details</param>
        /// <response code="200">User updated</response>
        /// <response code="400">Invalid data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                var currentUserId = GetCurrentUserId();

                var user = await _authService.UpdateUser(
                    id,
                    currentUserId,
                    request.DisplayName,
                    request.Email,
                    request.PhoneNumber,
                    request.IsActive
                );

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var role = await _authService.GetRoleById(user.RoleId);

                var response = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    RoleName = role?.RoleName ?? "Unknown",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Ok(response);
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
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Disable user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">User disabled</response>
        /// <response code="400">Invalid operation</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var success = await _authService.DisableUser(id, currentUserId);

                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User disabled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Role assignment details</param>
        /// <response code="200">Role assigned</response>
        /// <response code="400">Invalid operation</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Verify role exists
                var targetRole = await _authService.GetRoleById(request.RoleId);
                if (targetRole == null)
                {
                    return BadRequest(new { message = "Invalid role specified" });
                }

                var user = await _authService.AssignRole(id, request.RoleId, currentUserId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var response = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    RoleName = targetRole.RoleName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

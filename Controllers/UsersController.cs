using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers 
{
    /// <summary>
    /// 👥 Quản lý Người dùng
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UsersController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// 📋 Lấy danh sách người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var roles = await _roleService.GetAllAsync();

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
            }).OrderBy(u => u.Username).ToList();

            return Ok(response);
        }

        /// <summary>
        /// 🔍 Xem chi tiết người dùng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            var role = await _roleService.GetByIdAsync(user.RoleId);

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
        /// ➕ Tạo tài khoản mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var role = await _roleService.GetByIdAsync(request.RoleId);
                if (role == null) return BadRequest(new { message = "Invalid role specified" });

                var user = await _userService.CreateUserAsync(
                    request.Username,
                    request.Password,
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// ✏️ Cập nhật người dùng
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _userService.UpdateUserAsync(id, currentUserId, request.DisplayName, request.Email, request.PhoneNumber, request.IsActive);

                if (user == null) return NotFound(new { message = "User not found" });

                var role = await _roleService.GetByIdAsync(user.RoleId);

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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 🗑️ Vô hiệu hóa người dùng
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var success = await _userService.DisableUserAsync(id, currentUserId);
                if (!success) return NotFound(new { message = "User not found" });
                return Ok(new { message = "User disabled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 🎭 Gán vai trò
        /// </summary>
        [HttpPut("{id}/role")]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var currentUserId = GetCurrentUserId();
                var targetRole = await _roleService.GetByIdAsync(request.RoleId);
                if (targetRole == null) return BadRequest(new { message = "Invalid role specified" });

                var user = await _userService.AssignRoleAsync(id, request.RoleId, currentUserId);
                if (user == null) return NotFound(new { message = "User not found" });

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


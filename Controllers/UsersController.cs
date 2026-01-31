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
    [Authorize(Roles = "Admin")]
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
        /// 📋 Lấy danh sách người dùng
        /// </summary>
        /// <remarks>
        /// Chức năng: Lấy tất cả users  
        /// Quyền: Admin  
        /// Lỗi: 401, 403
        /// </remarks>
        /// <response code="200">Danh sách người dùng</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetAll()
        {
            var users = _authService.GetAllUsers();
            var roles = _authService.GetAllRoles();

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
        /// 🔍 Xem chi tiết người dùng
        /// </summary>
        /// <remarks>
        /// Chức năng: Lấy 1 user theo ID  
        /// Quyền: Admin  
        /// Lỗi: 401, 403, 404
        /// </remarks>
        /// <param name="id">ID người dùng</param>
        /// <response code="200">Thông tin người dùng</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id)
        {
            var user = _authService.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var role = _authService.GetRoleById(user.RoleId);

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
        /// <remarks>
        /// Chức năng: Tạo user mới với mật khẩu mặc định  
        /// Quyền: Admin  
        /// Body: username, roleId (bắt buộc), displayName, email, phoneNumber  
        /// Mật khẩu mặc định được gán tự động  
        /// User phải đổi mật khẩu khi đăng nhập lần đầu  
        /// Lỗi: 400 nếu username trùng, 401, 403
        /// </remarks>
        /// <param name="request">Thông tin tài khoản</param>
        /// <response code="201">Tạo thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verify role exists
                var role = _authService.GetRoleById(request.RoleId);
                if (role == null)
                {
                    return BadRequest(new { message = "Invalid role specified" });
                }

                var user = _authService.CreateUser(
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// ✏️ Cập nhật người dùng
        /// </summary>
        /// <remarks>
        /// Chức năng: Sửa thông tin user  
        /// Quyền: Admin  
        /// Body: displayName, email, phoneNumber, isActive  
        /// ⚠️ Admin KHÔNG THỂ disable chính mình  
        /// Lỗi: 400, 401, 403, 404
        /// </remarks>
        /// <param name="id">ID người dùng</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Vi phạm quy tắc</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                
                var user = _authService.UpdateUser(
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

                var role = _authService.GetRoleById(user.RoleId);

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
        /// <remarks>
        /// Chức năng: Set isActive = false  
        /// Quyền: Admin  
        /// ⚠️ Admin KHÔNG THỂ disable chính mình  
        /// Lỗi: 400, 401, 403, 404
        /// </remarks>
        /// <param name="id">ID người dùng</param>
        /// <response code="200">Vô hiệu hóa thành công</response>
        /// <response code="400">Vi phạm quy tắc</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var success = _authService.DisableUser(id, currentUserId);

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
        /// 🎭 Gán vai trò
        /// </summary>
        /// <remarks>
        /// Chức năng: Đổi role của user  
        /// Quyền: Admin  
        /// Body: roleId  
        /// ⚠️ Admin KHÔNG THỂ gỡ role Admin của chính mình  
        /// Lỗi: 400, 401, 403, 404
        /// </remarks>
        /// <param name="id">ID người dùng</param>
        /// <param name="request">RoleId mới</param>
        /// <response code="200">Gán thành công</response>
        /// <response code="400">Vi phạm quy tắc</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpPut("{id}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AssignRole(Guid id, [FromBody] AssignRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Verify role exists
                var targetRole = _authService.GetRoleById(request.RoleId);
                if (targetRole == null)
                {
                    return BadRequest(new { message = "Invalid role specified" });
                }

                var user = _authService.AssignRole(id, request.RoleId, currentUserId);

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

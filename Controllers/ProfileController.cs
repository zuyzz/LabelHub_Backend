using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Profile;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// 👤 Quản lý Profile Cá nhân
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
        /// 📝 Cập nhật thông tin cá nhân
        /// </summary>
        /// <remarks>
        /// Chức năng: Cho phép user tự cập nhật thông tin cá nhân  
        /// Điều kiện: Đã đổi mật khẩu lần đầu (IsFirstLogin = false)  
        /// Có thể cập nhật: DisplayName, Email, PhoneNumber  
        /// Không thể cập nhật: Username, Password, RoleId, IsActive  
        /// Quyền: User đã xác thực (token hợp lệ)  
        /// 
        /// Lỗi có thể xảy ra:
        /// - 400: Chưa đổi mật khẩu lần đầu
        /// - 401: Token không hợp lệ hoặc hết hạn
        /// - 404: User không tồn tại
        /// </remarks>
        /// <param name="request">Thông tin cần cập nhật (tất cả các field đều optional)</param>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Vi phạm business rules (chưa đổi mật khẩu)</response>
        /// <response code="401">Chưa xác thực hoặc token không hợp lệ</response>
        /// <response code="404">User không tồn tại</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
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
                var updatedUser = _authService.UpdateProfile(
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
                var role = _authService.GetRoleById(updatedUser.RoleId);

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
            catch (Exception ex)
            {
                // Handle business rule violations
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

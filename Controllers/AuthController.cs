using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// 🔐 Xác thực
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 🔑 Đăng nhập
        /// </summary>
        /// <remarks>
        /// Chức năng: Đăng nhập, trả JWT  
        /// Quyền: Public  
        /// Body: usernameOrEmail, password  
        /// 
        /// ⚠️ FIRST LOGIN FLOW:  
        /// - New users must change password on first login  
        /// - Login succeeds with requirePasswordChange = true  
        /// - User must call POST /api/auth/change-password before accessing other APIs  
        /// 
        /// Lỗi: 401 nếu sai thông tin
        /// </remarks>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <response code="200">Đăng nhập thành công, trả về thông tin user và JWT token</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="401">Sai thông tin đăng nhập hoặc tài khoản bị vô hiệu hóa</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = _authService.Login(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid credentials or account is inactive" });
            }

            return Ok(response);
        }

        /// <summary>
        /// 🔑 Change password on first login
        /// </summary>
        /// <remarks>
        /// Chức năng: Đổi mật khẩu lần đầu đăng nhập  
        /// Quyền: Authenticated user  
        /// Body: oldPassword, newPassword (bắt buộc)  
        /// Sau khi đổi thành công, user có thể truy cập API thông thường
        /// </remarks>
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
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = _authService.ChangePassword(
                userId,
                request.OldPassword,
                request.NewPassword
            );

            if (user == null)
            {
                return BadRequest(new { message = "Incorrect old password or user not found" });
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

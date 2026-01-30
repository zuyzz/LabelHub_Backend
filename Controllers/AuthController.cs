using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// 🔐 Xác thực
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid credentials or account is inactive" });
            }

            return Ok(response);
        }
    }
}

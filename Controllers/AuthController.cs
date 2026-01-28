using Microsoft.AspNetCore.Mvc;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Mock endpoint - Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // TODO: Implement real authentication logic with JWT
            return Ok(new
            {
                token = "mock-jwt-token",
                message = "Login successful (mock)"
            });
        }

        // Mock endpoint - Register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // TODO: Implement real user registration
            return Ok(new
            {
                message = "User registered successfully (mock)"
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

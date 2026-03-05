using DataLabelProject.Data;
using DataLabelProject.Application.DTOs.Auth;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DataLabelProject.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public AuthService(IConfiguration configuration, IUserRepository userRepo, IRoleRepository roleRepo, ITokenBlacklistService tokenBlacklistService)
        {
            _configuration = configuration;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _tokenBlacklistService = tokenBlacklistService;
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }

        /// <summary>
        /// Login method - validates user credentials and generates JWT token
        /// Validates username/email, password, and active status
        /// Returns specific error messages for different failure scenarios
        /// </summary>
        public async Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request)
        {
            // Find user by username or email (case-insensitive)
            var user = await _userRepo.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

            // Check if user exists
            if (user == null)
            {
                return (null, "Username or email not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return (null, "Account has been deactivated. Please contact administrator");
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return (null, "Incorrect password");
            }

            // Get role name
            var role = await _roleRepo.GetByIdAsync(user.RoleId);
            var roleName = role?.RoleName ?? "Unknown";

            // Generate JWT token
            var token = GenerateJwtToken(user.UserId, user.Username, roleName);
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

            // Check if first login
            var requirePasswordChange = user.IsFirstLogin;
            var message = requirePasswordChange 
                ? "Login successful. You must change your password before accessing other features."
                : "Login successful";

            var response = new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleName = roleName,
                Token = token,
                ExpiresAt = expiresAt,
                Message = message,
                RequirePasswordChange = requirePasswordChange
            };
            
            return (response, null);
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(Guid userId, string username, string roleName)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Change password for users
        /// Verifies old password before updating
        /// Sets IsFirstLogin = false
        /// Returns error message if validation fails
        /// </summary>
        public async Task<(User? User, string? ErrorMessage)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return (null, "User not found");
            }

            // Verify old password
            if (!VerifyPassword(oldPassword, user.PasswordHash))
            {
                return (null, "Incorrect old password");
            }

            // Check if new password is same as old
            if (VerifyPassword(newPassword, user.PasswordHash))
            {
                return (null, "New password cannot be the same as old password");
            }

            // Hash and update password
            user.PasswordHash = HashPassword(newPassword);
            user.IsFirstLogin = false;

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();
            return (user, null);
        }

        public Task<(bool Success, string? ErrorMessage)> LogoutAsync(string bearerToken)
        {
            if (string.IsNullOrWhiteSpace(bearerToken) || !bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult((Success: false, ErrorMessage: (string?)"Authorization header is missing or invalid"));
            }

            var token = bearerToken["Bearer ".Length..].Trim();
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return Task.FromResult((Success: false, ErrorMessage: (string?)"Invalid token"));
            }

            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(jti))
            {
                return Task.FromResult((Success: false, ErrorMessage: (string?)"Token identifier not found"));
            }

            return RevokeAndReturnAsync(jti, jwtToken.ValidTo.ToUniversalTime());
        }

        private async Task<(bool Success, string? ErrorMessage)> RevokeAndReturnAsync(string jti, DateTime expiresAtUtc)
        {
            await _tokenBlacklistService.RevokeTokenAsync(jti, expiresAtUtc);
            return (Success: true, ErrorMessage: null);
        }
    }
}

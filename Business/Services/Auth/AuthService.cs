using DataLabelProject.Application.DTOs.Auth;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace DataLabelProject.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IRefreshTokenRepository refreshTokenRepo,
            IJwtTokenService jwtTokenService)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtTokenService = jwtTokenService;
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
        /// Login method - validates user credentials and generates JWT + refresh tokens
        /// Prevents login when user.IsActive = false
        /// </summary>
        public async Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request)
        {
            // Find user by username or email
            var user = await _userRepo.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                return (null, "Username or email not found");
            }

            // Prevent login if user is disabled
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

            // Generate access token (15 minutes)
            var accessToken = _jwtTokenService.GenerateAccessToken(user.UserId, user.Username, roleName);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpiryMinutes());

            // Generate refresh token (7 days)
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);

            // Store refresh token in database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtTokenService.GetRefreshTokenExpiryDays())
            };

            await _refreshTokenRepo.CreateAsync(refreshTokenEntity);
            await _refreshTokenRepo.SaveChangesAsync();

            var response = new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleName = roleName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                Message = "Login successful"
            };

            return (response, null);
        }

        /// <summary>
        /// Refresh token flow - validates refresh token and generates new token pair
        /// Implements token rotation (old token revoked, new token issued)
        /// Rejects disabled users
        /// </summary>
        public async Task<(RefreshTokenResponse? Response, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);

            // Find refresh token in database
            var token = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);

            if (token == null)
            {
                return (null, "Invalid refresh token");
            }

            // Check if token is revoked
            if (token.RevokedAt != null)
            {
                return (null, "Refresh token has been revoked");
            }

            // Check if token is expired
            if (token.ExpiresAt < DateTime.UtcNow)
            {
                return (null, "Refresh token has expired");
            }

            // Check if user is still active
            var user = await _userRepo.GetByIdAsync(token.UserId);
            if (user == null || !user.IsActive)
            {
                return (null, "Account has been deactivated");
            }

            // Get role name
            var role = await _roleRepo.GetByIdAsync(user.RoleId);
            var roleName = role?.RoleName ?? "Unknown";

            // Generate new access token
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user.UserId, user.Username, roleName);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpiryMinutes());

            // Generate new refresh token (token rotation)
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshToken);

            // Revoke old refresh token
            token.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepo.UpdateAsync(token);

            // Store new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtTokenService.GetRefreshTokenExpiryDays())
            };
            token.ReplacedByToken = newRefreshTokenEntity.TokenId;
            await _refreshTokenRepo.CreateAsync(newRefreshTokenEntity);
            await _refreshTokenRepo.SaveChangesAsync();

            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = accessTokenExpiry
            };

            return (response, null);
        }

        /// <summary>
        /// Logout - revokes refresh token
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> LogoutAsync(LogoutRequest request)
        {
            var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);

            var token = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);

            if (token != null && token.RevokedAt == null)
            {
                token.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepo.UpdateAsync(token);
                await _refreshTokenRepo.SaveChangesAsync();
            }

            // Always return success even if token not found (idempotent operation)
            return (true, null);
        }

        /// <summary>
        /// Change password for users
        /// Verifies old password before updating
        /// Returns error message if validation fails
        /// </summary>
        public async Task<(User? User, string? ErrorMessage)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return (null, "User not found");
            }

            // Verify old password
            if (!VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return (null, "Incorrect old password");
            }

            // Check if new password is same as old
            if (VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                return (null, "New password cannot be the same as old password");
            }

            // Hash and update password
            user.PasswordHash = HashPassword(request.NewPassword);

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();
            return (user, null);
        }
    }
}

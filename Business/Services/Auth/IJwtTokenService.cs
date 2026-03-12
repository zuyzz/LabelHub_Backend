namespace DataLabelProject.Business.Services.Auth;

public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT access token with 15 minutes expiry
    /// </summary>
    string GenerateAccessToken(Guid userId, string username, string roleName);

    /// <summary>
    /// Generate secure random refresh token (base64 encoded)
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Hash refresh token using SHA256
    /// </summary>
    string HashRefreshToken(string refreshToken);

    /// <summary>
    /// Get access token expiry duration in minutes
    /// </summary>
    int GetAccessTokenExpiryMinutes();

    /// <summary>
    /// Get refresh token expiry duration in days
    /// </summary>
    int GetRefreshTokenExpiryDays();
}

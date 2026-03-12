using DataLabelProject.Application.DTOs.Auth;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Auth;

public interface IAuthService
{
    Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request);
    Task<(RefreshTokenResponse? Response, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenRequest request);
    Task<(bool Success, string? ErrorMessage)> LogoutAsync(LogoutRequest request);
    Task<(User? User, string? ErrorMessage)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

using DataLabelProject.Application.DTOs.Auth;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Auth;

public interface IAuthService
{
    Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request);
    Task<(User? User, string? ErrorMessage)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<(bool Success, string? ErrorMessage)> LogoutAsync(string bearerToken);
}

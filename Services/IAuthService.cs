using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services;

public interface IAuthService
{
    Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request);
    Task<(User? User, string? ErrorMessage)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
}

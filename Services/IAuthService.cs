using DataLabel_Project_BE.DTOs.Auth;

namespace DataLabel_Project_BE.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateUserAsync(string username, string password, string? displayName, string? email, string? phoneNumber, Guid roleId);
    Task<User?> UpdateUserAsync(Guid userId, Guid currentUserId, string? displayName, string? email, string? phoneNumber, bool? isActive);
    Task<bool> DisableUserAsync(Guid userId, Guid currentUserId);
    Task<User?> AssignRoleAsync(Guid userId, Guid roleId, Guid currentUserId);
}

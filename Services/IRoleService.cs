using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role> CreateRoleAsync(string roleName);
    Task<Role?> UpdateRoleAsync(Guid roleId, string roleName);
    Task<bool> DeleteRoleAsync(Guid roleId);
}

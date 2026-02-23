using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Roles;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role> CreateRoleAsync(string roleName);
    Task<Role?> UpdateRoleAsync(Guid roleId, string roleName);
    Task<bool> DeleteRoleAsync(Guid roleId);
}

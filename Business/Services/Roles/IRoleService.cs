using DataLabelProject.Application.DTOs.Roles;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Business.Services.Roles;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllRoles();
    Task<RoleResponse?> GetRoleById(Guid id);
    Task<RoleResponse> CreateRole(CreateRoleRequest request);
    Task<RoleResponse?> UpdateRole(Guid id, UpdateRoleRequest request);
    Task<bool> DeleteRole(Guid id);
}

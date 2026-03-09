using DataLabelProject.Application.DTOs.Roles;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Roles;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<RoleResponse>> GetAllRoles()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(MapToResponse).ToList();
    }

    public async Task<RoleResponse?> GetRoleById(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;
        
        return MapToResponse(role);
    }

    public async Task<RoleResponse> CreateRole(CreateRoleRequest request)
    {
        var existedRole = await _roleRepository.GetByNameAsync(request.RoleName);
        if (existedRole != null) throw new Exception("Role name already exists");

        var role = new Role 
        { 
            RoleId = Guid.NewGuid(), 
            RoleName = request.RoleName 
        };

        await _roleRepository.CreateAsync(role);
        await _roleRepository.SaveChangesAsync();

        return MapToResponse(role);
    }

    public async Task<RoleResponse?> UpdateRole(Guid id, UpdateRoleRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            var existedRole = await _roleRepository.GetByNameAsync(request.RoleName);
            if (existedRole != null && existedRole.RoleId != id) 
                throw new Exception("Role name already exists");
        }

        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            role.RoleName = request.RoleName;
        }

        await _roleRepository.UpdateAsync(role);
        await _roleRepository.SaveChangesAsync();

        return MapToResponse(role);
    }

    public async Task<bool> DeleteRole(Guid roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        var isInUse = await _roleRepository.IsRoleUsedAsync(roleId);
        if (isInUse) 
            throw new Exception("Cannot delete role because users are assigned to it");

        await _roleRepository.DeleteAsync(role);
        await _roleRepository.SaveChangesAsync();
        
        return true;
    }

    private RoleResponse MapToResponse(Role role)
    {
        return new RoleResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName
        };
    }
}

using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _roleRepo.GetAllAsync();
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _roleRepo.GetByIdAsync(id);
    }

    public async Task<Role> CreateRoleAsync(string roleName)
    {
        var exists = await _roleRepo.GetByNameAsync(roleName);
        if (exists != null) throw new Exception("Role name already exists");

        var role = new Role { RoleId = Guid.NewGuid(), RoleName = roleName };
        await _roleRepo.AddAsync(role);
        await _roleRepo.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> UpdateRoleAsync(Guid roleId, string roleName)
    {
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return null;

        var conflict = await _roleRepo.GetByNameAsync(roleName);
        if (conflict != null && conflict.RoleId != roleId) throw new Exception("Role name already exists");

        role.RoleName = roleName;
        await _roleRepo.UpdateAsync(role);
        await _roleRepo.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return false;

        var inUse = await _roleRepo.IsRoleUsedAsync(roleId);
        if (inUse) throw new Exception("Cannot delete role because users are assigned to it");

        await _roleRepo.DeleteAsync(role);
        await _roleRepo.SaveChangesAsync();
        return true;
    }
}

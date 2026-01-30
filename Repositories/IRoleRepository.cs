using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);
    Task<bool> IsRoleUsedAsync(Guid roleId);
    Task SaveChangesAsync();
}

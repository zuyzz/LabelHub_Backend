using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);
    Task<bool> IsRoleUsedAsync(Guid id);
    Task SaveChangesAsync();
}

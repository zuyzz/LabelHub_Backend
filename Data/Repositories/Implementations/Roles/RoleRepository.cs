using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Data.Repositories.Implementations.Roles;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => string.Equals(r.RoleName.ToLower(), name.ToLower()));
    }

    public async Task CreateAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
    }

    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
    }

    public async Task<bool> IsRoleUsedAsync(Guid id)
    {
        return await _context.Users
            .AnyAsync(u => u.RoleId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

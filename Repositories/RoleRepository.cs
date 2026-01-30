using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _db.Roles.AsNoTracking().ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Role role)
    {
        await _db.Roles.AddAsync(role);
    }

    public Task UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role)
    {
        _db.Roles.Remove(role);
        return Task.CompletedTask;
    }

    public async Task<bool> IsRoleUsedAsync(Guid roleId)
    {
        return await _db.Users.AnyAsync(u => u.RoleId == roleId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}

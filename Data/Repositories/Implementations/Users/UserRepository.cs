using DataLabelProject.Data;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using DataLabelProject.Application.DTOs.Users;

namespace DataLabelProject.Data.Repositories.Implementations.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(UserQueryParameters @params)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRole)
            .OrderByDescending(u => u.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(@params.Username))
            query = query.Where(u => EF.Functions.ILike(u.Username, $"%{@params.Username.Trim()}%"));

        if (!string.IsNullOrEmpty(@params.DisplayName))
            query = query.Where(u => EF.Functions.ILike(u.DisplayName, $"%{@params.DisplayName.Trim()}%"));

        if (!string.IsNullOrEmpty(@params.Email))
            query = query.Where(u => EF.Functions.ILike(u.Email ?? "", $"%{@params.Email.Trim()}%"));

        if (!string.IsNullOrEmpty(@params.PhoneNumber))
            query = query.Where(u => EF.Functions.ILike(u.PhoneNumber ?? "", $"%{@params.PhoneNumber.Trim()}%"));

        if (@params.IsActive.HasValue)
            query = query.Where(u => u.IsActive == @params.IsActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(@params.Offset)
            .Take(@params.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<User>> GetByIdsAsync(List<Guid> userIds)
    {
        return await _context.Users
            .Where(u => userIds.Contains(u.UserId))
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => 
                EF.Functions.ILike(u.Username, $"%{usernameOrEmail.Trim()}%") ||
                EF.Functions.ILike(u.Email ?? "", $"%{usernameOrEmail.Trim()}%"));
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, $"%{username.Trim()}%"));
    }

    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

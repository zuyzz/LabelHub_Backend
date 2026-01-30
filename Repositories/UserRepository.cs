using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLabel_Project_BE.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _db.Users.FirstOrDefaultAsync(u => 
            u.Username.ToLower() == usernameOrEmail.ToLower() || (u.Email != null && u.Email.ToLower() == usernameOrEmail.ToLower()));
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}

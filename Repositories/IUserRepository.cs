using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}

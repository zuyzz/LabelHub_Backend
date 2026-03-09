using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Repositories.Abstractions;

public interface IUserRepository
{
    Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(UserQueryParameters @params);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> GetByUsernameAsync(string username);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}

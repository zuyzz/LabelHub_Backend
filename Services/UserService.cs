using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public UserService(IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepo.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _userRepo.GetByIdAsync(id);
    }

    public async Task<User> CreateUserAsync(string username, string password, string? displayName, string? email, string? phoneNumber, Guid roleId)
    {
        // Check username
        var existing = await _userRepo.GetByUsernameAsync(username);
        if (existing != null)
        {
            throw new Exception("Username already exists");
        }

        // Verify role exists
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) {
            throw new Exception("Role not found");
        } else if (role.RoleName == "admin")
        {
            throw new Exception("Cannot create user with [admin] role");
        }

        // Hash password
        var passwordHash = AuthService.HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            DisplayName = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(Guid userId, Guid currentUserId, string? displayName, string? email, string? phoneNumber, bool? isActive)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        if (userId == currentUserId && isActive.HasValue && !isActive.Value)
        {
            throw new Exception("You cannot disable your own account");
        }

        if (displayName != null) user.DisplayName = displayName;
        if (email != null) user.Email = email;
        if (phoneNumber != null) user.PhoneNumber = phoneNumber;
        if (isActive.HasValue) user.IsActive = isActive.Value;

        await _userRepo.UpdateAsync(user);
        await _userRepo.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DisableUserAsync(Guid userId, Guid currentUserId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return false;

        if (userId == currentUserId)
        {
            throw new Exception("You cannot disable your own account");
        }

        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
        await _userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<User?> AssignRoleAsync(Guid userId, Guid roleId, Guid currentUserId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return null;

        if (userId == currentUserId)
        {
            var currentRole = await _roleRepo.GetByIdAsync(user.RoleId);
            if (currentRole?.RoleName == "admin" && role.RoleName != "admin")
            {
                throw new Exception("You cannot remove your own [admin] role");
            }
        }

        user.RoleId = roleId;
        await _userRepo.UpdateAsync(user);
        await _userRepo.SaveChangesAsync();

        return user;
    }
}

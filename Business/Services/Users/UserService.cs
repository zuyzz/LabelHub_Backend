using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Models;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<PagedResponse<UserResponse>> GetUsers(UserQueryParameters @params)
    {
        var (items, totalCount) = await _userRepository.GetAllAsync(@params);

        return new PagedResponse<UserResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalItems = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize,
        };
    }

    public async Task<UserResponse?> GetUserById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return MapToResponse(user);
    }

    public async Task<UserResponse> CreateUser(CreateUserRequest request)
    {
        var existedUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existedUser != null)
            throw new Exception("Username already exists");

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null) {
            throw new Exception("Role not found");
        } else if (role.RoleName == "admin")
            throw new Exception("Cannot create user with [admin] role");

        var passwordHash = Auth.AuthService.HashPassword(request.Password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHash,
            DisplayName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse?> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var currentUserId = _currentUserService.UserId!.Value;

        if (id == currentUserId && request.IsActive.HasValue)
            throw new Exception("You cannot activate/deactivate your own account");
        if (user.UserRole?.RoleName == "admin" && request.IsActive.HasValue)
            throw new Exception("You cannot activate/deactivate user with [admin] role");

        if (request.DisplayName != null) user.DisplayName = request.DisplayName;
        if (request.Email != null) user.Email = request.Email;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;

        // When admin disables user, revoke all refresh tokens
        if (request.IsActive.HasValue && request.IsActive.Value == false && user.IsActive == true)
        {
            user.IsActive = false;
            await _refreshTokenRepository.RevokeAllUserTokensAsync(id);
        }
        else if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.UserRole.RoleId,
            RoleName = user.UserRole.RoleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

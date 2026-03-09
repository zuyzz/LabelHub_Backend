using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Users;

namespace DataLabelProject.Business.Services.Users;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetUsers(UserQueryParameters @params);
    Task<UserResponse?> GetUserById(Guid id);
    Task<UserResponse> CreateUser(CreateUserRequest request);
    Task<UserResponse?> UpdateUser(Guid id, UpdateUserRequest request);
}

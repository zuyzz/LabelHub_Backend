using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Business.Services.Users
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User?.FindFirst("sub")?.Value;

                if (Guid.TryParse(claim, out var userId))
                    return userId;

                return null;
            }
        }

        public string? UserName
        {
            get
            {
                return User?.FindFirst(ClaimTypes.Name)?.Value
                    ?? User?.FindFirst("name")?.Value;
            }
        }

        public IEnumerable<string> Roles
        {
            get
            {
                return User?
                    .FindAll(ClaimTypes.Role)
                    .Select(r => r.Value)
                    ?? Enumerable.Empty<string>();
            }
        }
    }
}
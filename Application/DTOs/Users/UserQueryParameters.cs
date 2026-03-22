using DataLabelProject.Application.DTOs.Common;

namespace DataLabelProject.Application.DTOs.Users
{
    public class UserQueryParameters : PaginationParameters
    {
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
    }
}

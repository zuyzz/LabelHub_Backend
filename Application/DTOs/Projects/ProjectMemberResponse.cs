namespace DataLabelProject.Application.DTOs.Projects
{
    /// <summary>
    /// DTO for project member response with user details
    /// </summary>
    public class ProjectMemberResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}

namespace DataLabelProject.Application.DTOs.Roles
{
    public record RoleResponse
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}

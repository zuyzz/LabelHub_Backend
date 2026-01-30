namespace DataLabel_Project_BE.DTOs
{
    /// <summary>
    /// DTO for role response
    /// Clean response without navigation properties or entity relations
    /// </summary>
    public class RoleResponse
    {
        /// <summary>
        /// Unique identifier for the role
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Role name (e.g., Admin, Manager, Reviewer, Annotator)
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
    }
}

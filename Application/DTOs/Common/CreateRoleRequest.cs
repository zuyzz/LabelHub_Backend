using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Common
{
    /// <summary>
    /// DTO for creating a new role
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Role name (e.g., Admin, Manager, Reviewer, Annotator)
        /// </summary>
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
        public string RoleName { get; set; } = string.Empty;
    }
}

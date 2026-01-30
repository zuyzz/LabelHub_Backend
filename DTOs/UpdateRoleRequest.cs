using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs
{
    /// <summary>
    /// DTO for updating an existing role
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// Updated role name
        /// </summary>
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
        public string RoleName { get; set; } = string.Empty;
    }
}

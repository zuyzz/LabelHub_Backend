using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Roles
{
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; } = null!;
    }
}

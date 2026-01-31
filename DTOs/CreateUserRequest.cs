using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs
{
    /// <summary>
    /// DTO for creating a new user (Admin only)
    /// Password is automatically set to default value
    /// </summary>
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "RoleId is required")]
        public Guid RoleId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace DataLabel_Project_BE.DTOs
{
    /// <summary>
    /// DTO for updating user information (Admin only)
    /// </summary>
    public class UpdateUserRequest
    {
        [StringLength(100)]
        public string? DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
    }
}

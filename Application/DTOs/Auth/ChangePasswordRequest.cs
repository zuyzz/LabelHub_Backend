using System.ComponentModel.DataAnnotations;

namespace DataLabelProject.Application.DTOs.Auth
{
    /// <summary>
    /// Request to change password on first login
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

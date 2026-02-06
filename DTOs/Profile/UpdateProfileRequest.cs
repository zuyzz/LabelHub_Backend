namespace DataLabel_Project_BE.DTOs.Profile
{
    /// <summary>
    /// DTO for updating user profile (self-update only)
    /// All fields are optional - only provided fields will be updated
    /// </summary>
    public class UpdateProfileRequest
    {
        /// <summary>
        /// Tên hiển thị
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}

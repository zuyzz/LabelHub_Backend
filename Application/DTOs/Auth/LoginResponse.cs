namespace DataLabelProject.Application.DTOs.Auth
{
    public record LoginResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

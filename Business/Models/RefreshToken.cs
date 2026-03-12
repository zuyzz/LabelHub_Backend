using System;

namespace DataLabelProject.Business.Models;

public class RefreshToken
{
    public Guid TokenId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// SHA256 hash of the refresh token
    /// </summary>
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when token was revoked (logout or admin disabled user)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token ID that replaced this token during rotation
    /// </summary>
    public Guid? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Check if token is active (not revoked and not expired)
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}

using Asambleas.Common.Entities;

namespace Asambleas.Features.Auth.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Tracks token rotation chain. When this token is rotated,
    /// this field stores the replacement token value.
    /// Used for reuse detection: if a revoked token is presented,
    /// we revoke the entire family (all tokens for the user).
    /// </summary>
    public string? ReplacedByToken { get; set; }

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

using Asambleas.Common.Entities;

namespace Asambleas.Features.Auth.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// Implementación de IRefreshTokenRepository usando EF Core.
/// </summary>
public class RefreshTokenRepository(ApplicationDbContext db) : IRefreshTokenRepository
{
    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<RefreshToken?> GetByTokenValueAsync(Guid userId, string tokenValue, CancellationToken ct = default)
        => await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == tokenValue, ct);

    public async Task<int> RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsRevoked, true)
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);

    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);
}

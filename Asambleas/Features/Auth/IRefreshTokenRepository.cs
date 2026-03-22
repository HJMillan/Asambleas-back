using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Abstracción de acceso a datos para refresh tokens.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Obtiene tokens activos (no revocados, no expirados) de un usuario, ordenados por creación.
    /// </summary>
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Busca un refresh token específico por valor y userId.
    /// </summary>
    Task<RefreshToken?> GetByTokenValueAsync(Guid userId, string tokenValue, CancellationToken ct = default);

    /// <summary>
    /// Revoca todos los tokens activos de un usuario.
    /// Retorna la cantidad de tokens revocados.
    /// </summary>
    Task<int> RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo refresh token.
    /// </summary>
    void Add(RefreshToken token);
}

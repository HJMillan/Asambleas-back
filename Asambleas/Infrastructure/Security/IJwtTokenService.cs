using System.Security.Claims;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Infrastructure.Security;

/// <summary>
/// Abstracción del servicio de generación y validación de JWT tokens.
/// Permite testabilidad sin depender directamente de la implementación.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un access token JWT para el usuario dado.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Genera un refresh token criptográficamente seguro.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Valida un access token expirado y extrae su ClaimsPrincipal.
    /// Retorna null si el token es inválido.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

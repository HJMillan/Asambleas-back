using Asambleas.Common;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Interfaz del servicio de autenticación para inversión de dependencias y testabilidad.
/// Usa Result&lt;T&gt; para errores de negocio en lugar de excepciones.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario con rol DOCENTE.
    /// </summary>
    Task<Result<(User User, string AccessToken, string RefreshToken)>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Autenticación por CUIL + contraseña.
    /// </summary>
    Task<Result<(User User, string AccessToken, string RefreshToken)>> LoginCuilAsync(LoginCuilRequest request);

    /// <summary>
    /// Autenticación por credenciales LDAP (usuario de red).
    /// </summary>
    Task<Result<(User User, string AccessToken, string RefreshToken)>> LoginLdapAsync(LoginLdapRequest request);

    /// <summary>
    /// Renueva tokens usando un access token expirado + refresh token válido.
    /// Implementa rotación de tokens y detección de reutilización.
    /// </summary>
    Task<Result<(User User, string AccessToken, string NewRefreshToken)>> RefreshTokenAsync(string expiredAccessToken, string refreshTokenValue);

    /// <summary>
    /// Cierra la sesión actual revocando el refresh token proporcionado.
    /// </summary>
    Task LogoutAsync(Guid userId, string? refreshTokenValue);

    /// <summary>
    /// Revoca todos los refresh tokens activos del usuario (cierra todas las sesiones).
    /// </summary>
    Task RevokeAllTokensAsync(Guid userId);

    /// <summary>
    /// Obtiene un usuario por ID (solo lectura).
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId);
}

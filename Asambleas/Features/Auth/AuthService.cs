using System.Security.Cryptography;
using System.Text;
using Asambleas.Common;
using Asambleas.Features.Auth.Entities;
using Asambleas.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Asambleas.Features.Auth;

/// <summary>
/// Servicio de autenticación. Usa repositorios para acceso a datos y Result&lt;T&gt;
/// para errores de negocio, reservando excepciones solo para errores inesperados.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwt;
    private readonly JwtSettings _jwtSettings;
    private readonly ILdapService _ldap;
    private readonly IDomelecService _domelec;
    private readonly ILogger<AuthService> _logger;

    private const int MaxLoginAttempts = 5;
    private const int MaxActiveRefreshTokens = 5;

    /// <summary>
    /// Constructor con inyección de dependencias.
    /// </summary>
    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository tokens,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwt,
        IOptions<JwtSettings> jwtSettings,
        ILdapService ldap,
        IDomelecService domelec,
        ILogger<AuthService> logger)
    {
        _users = users;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
        _jwt = jwt;
        _jwtSettings = jwtSettings.Value;
        _ldap = ldap;
        _domelec = domelec;
        _logger = logger;
    }

    // ── Register ──

    /// <inheritdoc />
    public async Task<Result<(User User, string AccessToken, string RefreshToken)>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _users.GetByCuilAsync(request.Cuil);
        if (existingUser != null)
            return Result<(User, string, string)>.Conflict("Ya existe un usuario con este CUIL.");

        var existingEmail = await _users.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            return Result<(User, string, string)>.Conflict("Ya existe un usuario con este email.");

        var dni = request.Cuil.Substring(2, 8);

        var user = new User
        {
            Cuil = request.Cuil,
            Dni = dni,
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.DOCENTE
        };

        user.IsDomelecVerified = await _domelec.VerificarAsync(user.Cuil);

        _users.Add(user);

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Nuevo usuario registrado. UserId={UserId}, Role={Role}",
            user.Id, user.Role);

        return Result<(User, string, string)>.Success((user, accessToken, refreshToken));
    }

    // ── Login CUIL ──

    /// <inheritdoc />
    public async Task<Result<(User User, string AccessToken, string RefreshToken)>> LoginCuilAsync(LoginCuilRequest request)
    {
        var user = await _users.GetByCuilAsync(request.Cuil);
        if (user == null)
        {
            _logger.LogWarning(
                "Intento de login fallido: CUIL no encontrado. CUIL={CuilPrefix}***",
                request.Cuil[..3]);
            return Result<(User, string, string)>.Failure("CUIL o contraseña incorrectos.", 401);
        }

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingSeconds = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds;
            _logger.LogWarning(
                "Intento de login en cuenta bloqueada. UserId={UserId}, RemainingSeconds={Remaining}",
                user.Id, remainingSeconds);
            return Result<(User, string, string)>.Failure($"Cuenta bloqueada. Intente nuevamente en {remainingSeconds} segundos.", 429);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.LoginAttempts++;
            if (user.LoginAttempts >= MaxLoginAttempts)
            {
                var lockoutDuration = GetLockoutDuration(user.ConsecutiveLockouts);
                user.LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
                user.ConsecutiveLockouts++;
                user.LoginAttempts = 0;
                _logger.LogWarning(
                    "Cuenta bloqueada por exceso de intentos. UserId={UserId}, LockoutDuration={Duration}, ConsecutiveLockouts={Consecutive}",
                    user.Id, lockoutDuration, user.ConsecutiveLockouts);
            }
            else
            {
                _logger.LogWarning(
                    "Intento de login fallido (contraseña incorrecta). UserId={UserId}, Attempt={Attempt}/{Max}",
                    user.Id, user.LoginAttempts, MaxLoginAttempts);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<(User, string, string)>.Failure("CUIL o contraseña incorrectos.", 401);
        }

        // Reset login attempts and consecutive lockouts on success
        user.LoginAttempts = 0;
        user.ConsecutiveLockouts = 0;
        user.LockoutEnd = null;

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Login exitoso. UserId={UserId}", user.Id);

        return Result<(User, string, string)>.Success((user, accessToken, refreshToken));
    }

    // ── Login LDAP ──

    /// <inheritdoc />
    public async Task<Result<(User User, string AccessToken, string RefreshToken)>> LoginLdapAsync(LoginLdapRequest request)
    {
        var ldapResult = await _ldap.AuthenticateAsync(request.Username, request.Password);
        if (ldapResult == null)
        {
            _logger.LogWarning(
                "Intento de login LDAP fallido. Username={Username}",
                request.Username);
            return Result<(User, string, string)>.Failure("Credenciales LDAP incorrectas.", 401);
        }

        var user = await _users.GetByLdapUsernameAsync(request.Username);

        if (user == null)
        {
            user = new User
            {
                LdapUsername = request.Username,
                Nombre = ldapResult.Nombre,
                Apellido = ldapResult.Apellido,
                Email = ldapResult.Email,
                Dni = ldapResult.Dni,
                Cuil = ldapResult.Cuil,
                PasswordHash = string.Empty,
                Role = Role.OPERADOR,
                IsDomelecVerified = true
            };
            _users.Add(user);

            _logger.LogInformation(
                "Nuevo usuario LDAP creado. Username={Username}, Role={Role}",
                request.Username, user.Role);
        }

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Login LDAP exitoso. UserId={UserId}", user.Id);

        return Result<(User, string, string)>.Success((user, accessToken, refreshToken));
    }

    // ── Refresh ──

    /// <inheritdoc />
    public async Task<Result<(User User, string AccessToken, string NewRefreshToken)>> RefreshTokenAsync(string expiredAccessToken, string refreshTokenValue)
    {
        var principal = _jwt.GetPrincipalFromExpiredToken(expiredAccessToken);
        if (principal == null)
            return Result<(User, string, string)>.Failure("Token inválido.", 401);

        // Validar antigüedad del access_token expirado (máximo 1 día)
        var expClaim = principal.FindFirst("exp")?.Value;
        if (expClaim != null && long.TryParse(expClaim, out var exp))
        {
            var expiredAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            if (expiredAt < DateTime.UtcNow.AddDays(-1))
                return Result<(User, string, string)>.Failure("Token demasiado antiguo para renovar. Inicie sesión nuevamente.", 401);
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Result<(User, string, string)>.Failure("Token inválido.", 401);

        var user = await _users.GetByIdWithTokensAsync(userId);

        if (user == null)
            return Result<(User, string, string)>.NotFound("Usuario no encontrado.");

        // Buscar por hash SHA-256 del refresh token presentado
        var tokenHash = HashRefreshToken(refreshTokenValue);
        var storedToken = user.RefreshTokens
            .FirstOrDefault(t => t.Token == tokenHash);

        if (storedToken == null)
            return Result<(User, string, string)>.Failure("Refresh token inválido.", 401);

        // ── DETECCIÓN DE REUTILIZACIÓN ──
        if (storedToken.IsRevoked)
        {
            _logger.LogCritical(
                "⚠️ REUSO DE REFRESH TOKEN DETECTADO. Posible robo de tokens. " +
                "UserId={UserId}, TokenId={TokenId}, FamilyId={FamilyId}. Revocando todos los tokens del usuario.",
                userId, storedToken.Id, storedToken.FamilyId);

            foreach (var token in user.RefreshTokens.Where(t => !t.IsRevoked))
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<(User, string, string)>.Failure("Sesión comprometida. Por seguridad, se cerraron todas las sesiones.", 401);
        }

        var margin = TimeSpan.FromMinutes(5);
        if (storedToken.ExpiresAt.Add(margin) < DateTime.UtcNow)
            return Result<(User, string, string)>.Failure("Refresh token expirado.", 401);

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var (accessToken, newRefreshToken) = await GenerateTokensAsync(user, storedToken.FamilyId);

        storedToken.ReplacedByToken = HashRefreshToken(newRefreshToken);

        await _unitOfWork.SaveChangesAsync();

        return Result<(User, string, string)>.Success((user, accessToken, newRefreshToken));
    }

    // ── Logout ──

    /// <inheritdoc />
    public async Task LogoutAsync(Guid userId, string? refreshTokenValue)
    {
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var tokenHash = HashRefreshToken(refreshTokenValue);
            var token = await _tokens.GetByTokenValueAsync(userId, tokenHash);

            if (token is { IsRevoked: false })
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Logout. UserId={UserId}", userId);
    }

    // ── Revoke All Sessions ──

    /// <inheritdoc />
    public async Task RevokeAllTokensAsync(Guid userId)
    {
        var revoked = await _tokens.RevokeAllByUserIdAsync(userId);

        _logger.LogWarning(
            "Todas las sesiones revocadas. UserId={UserId}, TokensRevocados={Count}",
            userId, revoked);
    }

    // ── Get User ──

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _users.GetByIdReadOnlyAsync(userId);
    }

    // ── Helpers ──

    /// <summary>
    /// Lockout escalado — cada lockout consecutivo aumenta la duración.
    /// </summary>
    private static TimeSpan GetLockoutDuration(int consecutiveLockouts) => consecutiveLockouts switch
    {
        0 => TimeSpan.FromSeconds(30),
        1 => TimeSpan.FromMinutes(5),
        2 => TimeSpan.FromMinutes(15),
        _ => TimeSpan.FromHours(1)
    };

    /// <summary>
    /// Genera par de tokens y revoca tokens antiguos si exceden el máximo permitido.
    /// Nota: La query separada para tokens activos es intencional (ver ADR-003 #15).
    /// </summary>
    private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user, string? familyId = null)
    {
        var activeTokens = await _tokens.GetActiveTokensByUserIdAsync(user.Id);

        if (activeTokens.Count >= MaxActiveRefreshTokens)
        {
            foreach (var old in activeTokens.Take(activeTokens.Count - MaxActiveRefreshTokens + 1))
            {
                old.IsRevoked = true;
                old.RevokedAt = DateTime.UtcNow;
            }
        }

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = HashRefreshToken(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            UserId = user.Id,
            FamilyId = familyId ?? Guid.NewGuid().ToString("N")
        };

        _tokens.Add(refreshToken);

        return (accessToken, refreshTokenValue);
    }

    /// <summary>
    /// SHA-256 hash de refresh token para almacenamiento seguro en DB.
    /// Si la DB se compromete, los tokens hasheados no son usables.
    /// </summary>
    private static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}

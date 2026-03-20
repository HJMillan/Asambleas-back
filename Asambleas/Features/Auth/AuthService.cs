using Asambleas.Features.Auth.Entities;
using Asambleas.Infrastructure.Database;
using Asambleas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Asambleas.Features.Auth;

public class AuthService
{
    private readonly ApplicationDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly JwtSettings _jwtSettings;
    private readonly ILdapService _ldap;
    private readonly IDomelecService _domelec;

    private const int MaxLoginAttempts = 5;
    private const int LockoutSeconds = 30;

    public AuthService(
        ApplicationDbContext db,
        JwtTokenService jwt,
        IOptions<JwtSettings> jwtSettings,
        ILdapService ldap,
        IDomelecService domelec)
    {
        _db = db;
        _jwt = jwt;
        _jwtSettings = jwtSettings.Value;
        _ldap = ldap;
        _domelec = domelec;
    }

    // ── Register ──

    public async Task<(User User, string AccessToken, string RefreshToken)> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Las contraseñas no coinciden.");

        if (request.Password.Length < 6)
            throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

        if (request.Cuil.Length != 11 || !request.Cuil.All(char.IsDigit))
            throw new ArgumentException("El CUIL debe tener 11 dígitos.");

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Cuil == request.Cuil);
        if (existingUser != null)
            throw new InvalidOperationException("Ya existe un usuario con este CUIL.");

        var existingEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("Ya existe un usuario con este email.");

        // Extract DNI from CUIL (positions 2-9)
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

        // Domelec verification (stub for now)
        user.IsDomelecVerified = await _domelec.VerificarAsync(user.Cuil);

        _db.Users.Add(user);

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);

        await _db.SaveChangesAsync();

        return (user, accessToken, refreshToken);
    }

    // ── Login CUIL ──

    public async Task<(User User, string AccessToken, string RefreshToken)> LoginCuilAsync(LoginCuilRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Cuil == request.Cuil);
        if (user == null)
            throw new UnauthorizedAccessException("CUIL o contraseña incorrectos.");

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingSeconds = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds;
            throw new UnauthorizedAccessException($"Cuenta bloqueada. Intente nuevamente en {remainingSeconds} segundos.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.LoginAttempts++;
            if (user.LoginAttempts >= MaxLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(LockoutSeconds);
                user.LoginAttempts = 0;
            }
            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("CUIL o contraseña incorrectos.");
        }

        // Reset login attempts on success
        user.LoginAttempts = 0;
        user.LockoutEnd = null;

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);
        await _db.SaveChangesAsync();

        return (user, accessToken, refreshToken);
    }

    // ── Login LDAP ──

    public async Task<(User User, string AccessToken, string RefreshToken)> LoginLdapAsync(LoginLdapRequest request)
    {
        var ldapResult = await _ldap.AuthenticateAsync(request.Username, request.Password);
        if (ldapResult == null)
            throw new UnauthorizedAccessException("Credenciales LDAP incorrectas.");

        // Find or create user from LDAP data
        var user = await _db.Users.FirstOrDefaultAsync(u => u.LdapUsername == request.Username);

        if (user == null)
        {
            // First LDAP login: create user
            user = new User
            {
                LdapUsername = request.Username,
                Nombre = ldapResult.Nombre,
                Apellido = ldapResult.Apellido,
                Email = ldapResult.Email,
                Dni = ldapResult.Dni,
                Cuil = ldapResult.Cuil,
                PasswordHash = string.Empty, // No password for LDAP users
                Role = Role.OPERADOR, // Default role for LDAP users (municipal staff)
                IsDomelecVerified = true
            };
            _db.Users.Add(user);
        }

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);
        await _db.SaveChangesAsync();

        return (user, accessToken, refreshToken);
    }

    // ── Refresh ──

    public async Task<(User User, string AccessToken, string NewRefreshToken)> RefreshTokenAsync(string expiredAccessToken, string refreshTokenValue)
    {
        var principal = _jwt.GetPrincipalFromExpiredToken(expiredAccessToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Token inválido.");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Token inválido.");

        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new UnauthorizedAccessException("Usuario no encontrado.");

        var storedToken = user.RefreshTokens
            .FirstOrDefault(t => t.Token == refreshTokenValue && !t.IsRevoked);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Refresh token inválido o revocado.");

        // Allow refresh within a 5-minute margin after expiration
        var margin = TimeSpan.FromMinutes(5);
        if (storedToken.ExpiresAt.Add(margin) < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expirado.");

        // Revoke old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var (accessToken, newRefreshToken) = await GenerateTokensAsync(user);
        await _db.SaveChangesAsync();

        return (user, accessToken, newRefreshToken);
    }

    // ── Logout ──

    public async Task LogoutAsync(Guid userId, string? refreshTokenValue)
    {
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshTokenValue && !t.IsRevoked);

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }

    // ── Get User ──

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _db.Users.FindAsync(userId);
    }

    // ── Helpers ──

    private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            UserId = user.Id
        };

        _db.RefreshTokens.Add(refreshToken);

        return (accessToken, refreshTokenValue);
    }

    public static UserInfoResponse MapToResponse(User user) => new(
        Id: user.Id,
        Dni: user.Dni,
        Cuil: user.Cuil,
        Nombre: user.Nombre,
        Apellido: user.Apellido,
        Email: user.Email,
        Role: user.Role.ToString(),
        IsDomelecVerified: user.IsDomelecVerified
    );
}

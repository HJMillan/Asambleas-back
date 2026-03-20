using System.Security.Claims;
using Asambleas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Asambleas.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(AuthService authService, IOptions<JwtSettings> jwtSettings)
    {
        _authService = authService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Registro de nuevo usuario (siempre con rol DOCENTE).
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var (user, accessToken, refreshToken) = await _authService.RegisterAsync(request);

        CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings);

        return Ok(new AuthResponse(
            User: AuthService.MapToResponse(user),
            Message: "Registro exitoso."
        ));
    }

    /// <summary>
    /// Login por CUIL + contraseña.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginCuil([FromBody] LoginCuilRequest request)
    {
        var (user, accessToken, refreshToken) = await _authService.LoginCuilAsync(request);

        CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings);

        return Ok(new AuthResponse(
            User: AuthService.MapToResponse(user),
            Message: "Login exitoso."
        ));
    }

    /// <summary>
    /// Login por usuario de red LDAP.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login/ldap")]
    public async Task<ActionResult<AuthResponse>> LoginLdap([FromBody] LoginLdapRequest request)
    {
        var (user, accessToken, refreshToken) = await _authService.LoginLdapAsync(request);

        CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings);

        return Ok(new AuthResponse(
            User: AuthService.MapToResponse(user),
            Message: "Login LDAP exitoso."
        ));
    }

    /// <summary>
    /// Refresh de tokens (lee refresh_token de cookie).
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        var expiredAccessToken = CookieHelper.GetAccessToken(Request);
        var refreshTokenValue = CookieHelper.GetRefreshToken(Request);

        if (string.IsNullOrEmpty(expiredAccessToken) || string.IsNullOrEmpty(refreshTokenValue))
            return Unauthorized(new { message = "Tokens no encontrados." });

        var (user, accessToken, newRefreshToken) = await _authService.RefreshTokenAsync(expiredAccessToken, refreshTokenValue);

        CookieHelper.SetTokenCookies(Response, accessToken, newRefreshToken, _jwtSettings);

        return Ok(new AuthResponse(
            User: AuthService.MapToResponse(user),
            Message: "Token actualizado."
        ));
    }

    /// <summary>
    /// Logout: limpia cookies y revoca refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var refreshTokenValue = CookieHelper.GetRefreshToken(Request);

        await _authService.LogoutAsync(userId, refreshTokenValue);

        CookieHelper.ClearTokenCookies(Response);

        return Ok(new { message = "Sesión cerrada." });
    }

    /// <summary>
    /// Retorna los datos del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfoResponse>> Me()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado." });

        return Ok(AuthService.MapToResponse(user));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Token inválido.");
        return userId;
    }
}

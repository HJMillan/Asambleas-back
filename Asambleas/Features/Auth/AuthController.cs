using System.Security.Claims;
using Asambleas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Asambleas.Features.Auth;

/// <summary>
/// Controller de autenticación. Maneja registro, login (CUIL y LDAP),
/// refresh de tokens, logout y consulta de perfil.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtSettings _jwtSettings;
    private readonly bool _isSecure;

    /// <summary>
    /// Constructor con inyección de dependencias.
    /// </summary>
    public AuthController(IAuthService authService, IOptions<JwtSettings> jwtSettings, IWebHostEnvironment env)
    {
        _authService = authService;
        _jwtSettings = jwtSettings.Value;
        _isSecure = !env.IsDevelopment();
    }

    /// <summary>
    /// Registro de nuevo usuario (siempre con rol DOCENTE).
    /// </summary>
    /// <param name="request">Datos de registro: CUIL, nombre, apellido, email, contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos del usuario creado y mensaje de éxito.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request);

        return result.Match<ActionResult<AuthResponse>>(
            onSuccess: tuple =>
            {
                var (user, accessToken, refreshToken) = tuple;
                CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings, _isSecure);
                return Ok(new AuthResponse(
                    User: user.ToResponse(),
                    Message: "Registro exitoso."
                ));
            },
            onFailure: error => error.Contains("CUIL") || error.Contains("email")
                ? Conflict(CreateProblem(409, "Entidad duplicada", error))
                : BadRequest(CreateProblem(400, "Solicitud inválida", error))
        );
    }

    /// <summary>
    /// Login por CUIL + contraseña.
    /// </summary>
    /// <param name="request">CUIL (11 dígitos) y contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos del usuario y mensaje de éxito.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> LoginCuil([FromBody] LoginCuilRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginCuilAsync(request);

        return result.Match<ActionResult<AuthResponse>>(
            onSuccess: tuple =>
            {
                var (user, accessToken, refreshToken) = tuple;
                CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings, _isSecure);
                return Ok(new AuthResponse(
                    User: user.ToResponse(),
                    Message: "Login exitoso."
                ));
            },
            onFailure: error => Unauthorized(CreateProblem(401, "No autorizado", error))
        );
    }

    /// <summary>
    /// Login por usuario de red LDAP.
    /// </summary>
    /// <param name="request">Username y contraseña de red.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos del usuario y mensaje de éxito.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login/ldap")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> LoginLdap([FromBody] LoginLdapRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginLdapAsync(request);

        return result.Match<ActionResult<AuthResponse>>(
            onSuccess: tuple =>
            {
                var (user, accessToken, refreshToken) = tuple;
                CookieHelper.SetTokenCookies(Response, accessToken, refreshToken, _jwtSettings, _isSecure);
                return Ok(new AuthResponse(
                    User: user.ToResponse(),
                    Message: "Login LDAP exitoso."
                ));
            },
            onFailure: error => Unauthorized(CreateProblem(401, "No autorizado", error))
        );
    }

    /// <summary>
    /// Refresh de tokens (lee refresh_token de cookie).
    /// Rota el refresh token y genera un nuevo access token.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos del usuario y mensaje de éxito.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var expiredAccessToken = CookieHelper.GetAccessToken(Request);
        var refreshTokenValue = CookieHelper.GetRefreshToken(Request);

        if (string.IsNullOrEmpty(expiredAccessToken) || string.IsNullOrEmpty(refreshTokenValue))
            return Unauthorized(CreateProblem(401, "No autorizado", "Tokens no encontrados."));

        var result = await _authService.RefreshTokenAsync(expiredAccessToken, refreshTokenValue);

        return result.Match<ActionResult<AuthResponse>>(
            onSuccess: tuple =>
            {
                var (user, accessToken, newRefreshToken) = tuple;
                CookieHelper.SetTokenCookies(Response, accessToken, newRefreshToken, _jwtSettings, _isSecure);
                return Ok(new AuthResponse(
                    User: user.ToResponse(),
                    Message: "Token actualizado."
                ));
            },
            onFailure: error => Unauthorized(CreateProblem(401, "No autorizado", error))
        );
    }

    /// <summary>
    /// Logout: limpia cookies y revoca refresh token.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Mensaje de confirmación.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var refreshTokenValue = CookieHelper.GetRefreshToken(Request);

        await _authService.LogoutAsync(userId, refreshTokenValue);

        CookieHelper.ClearTokenCookies(Response);

        return Ok(new { message = "Sesión cerrada." });
    }

    /// <summary>
    /// Cierra todas las sesiones activas del usuario (revoca todos los refresh tokens).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Mensaje de confirmación.</returns>
    [Authorize]
    [HttpPost("revoke-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _authService.RevokeAllTokensAsync(userId);

        CookieHelper.ClearTokenCookies(Response);

        return Ok(new { message = "Todas las sesiones fueron cerradas." });
    }

    /// <summary>
    /// Retorna los datos del usuario autenticado.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información del perfil del usuario.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoResponse>> Me(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetByIdAsync(userId);

        if (user == null)
            return NotFound(CreateProblem(404, "No encontrado", "Usuario no encontrado."));

        return Ok(user.ToResponse());
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Token inválido.");
        return userId;
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail) => new()
    {
        Type = $"https://httpstatuses.com/{status}",
        Title = title,
        Status = status,
        Detail = detail
    };
}

using Microsoft.Extensions.Options;

namespace Asambleas.Infrastructure.Security;

public static class CookieHelper
{
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";
    // Usar /api/ para cubrir todas las versiones (/api/v1/auth, /api/v2/auth, etc.)
    private const string RefreshTokenPath = "/api/";

    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, JwtSettings settings, bool isSecure = true)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = RefreshTokenPath,
            Expires = DateTime.UtcNow.AddDays(settings.RefreshTokenExpirationDays)
        };

        response.Cookies.Append(AccessTokenCookie, accessToken, accessCookieOptions);
        response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);
    }

    public static void ClearTokenCookies(HttpResponse response)
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        response.Cookies.Append(AccessTokenCookie, string.Empty, expiredOptions);
        response.Cookies.Append(RefreshTokenCookie, string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenPath,
            Expires = DateTime.UtcNow.AddDays(-1)
        });
    }

    public static string? GetAccessToken(HttpRequest request)
    {
        return request.Cookies[AccessTokenCookie];
    }

    public static string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookie];
    }
}

using Microsoft.Extensions.Options;

namespace Asambleas.Infrastructure.Security;

public static class CookieHelper
{
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";

    public static void SetTokenCookies(HttpResponse response, string accessToken, string refreshToken, JwtSettings settings)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
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
            Path = "/api/auth",
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

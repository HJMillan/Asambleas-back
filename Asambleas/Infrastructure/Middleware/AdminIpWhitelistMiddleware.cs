namespace Asambleas.Infrastructure.Middleware;

/// <summary>
/// H10: Middleware que restringe el acceso a endpoints de administración
/// basado en la IP del cliente. IPs permitidas se configuran en appsettings.
/// En producción: si no hay IPs configuradas, bloquea por defecto (secure by default).
/// En desarrollo: si no hay IPs configuradas, permite todo.
/// </summary>
public class AdminIpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _allowedIps;
    private readonly ILogger<AdminIpWhitelistMiddleware> _logger;
    private readonly bool _isProduction;

    public AdminIpWhitelistMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<AdminIpWhitelistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _isProduction = env.IsProduction();
        _allowedIps = configuration.GetSection("AdminSettings:AllowedIPs")
            .Get<string[]>()?.ToHashSet() ?? [];

        if (_allowedIps.Count == 0 && _isProduction)
        {
            _logger.LogWarning(
                "AdminIpWhitelistMiddleware: No hay IPs configuradas en AdminSettings:AllowedIPs. " +
                "Los endpoints /api/admin serán BLOQUEADOS en producción.");
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
        {
            // En producción, bloquear si no hay whitelist configurada
            if (_allowedIps.Count == 0 && _isProduction)
            {
                _logger.LogWarning(
                    "Acceso administrativo bloqueado (no hay whitelist). Path={Path}", path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (_allowedIps.Count > 0)
            {
                var remoteIp = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();

                if (remoteIp == null || !_allowedIps.Contains(remoteIp))
                {
                    _logger.LogWarning(
                        "Acceso administrativo denegado. IP={RemoteIp}, Path={Path}",
                        remoteIp, path);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
        }

        await _next(context);
    }
}

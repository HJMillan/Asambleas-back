namespace Asambleas.Infrastructure.Middleware;

/// <summary>
/// Agrega headers de seguridad HTTP recomendados por OWASP a todas las respuestas.
/// Protege contra clickjacking, MIME-sniffing, y ataques de inyección.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // ── Headers de seguridad estándar ──

        // Previene que el navegador interprete archivos con un MIME type diferente al declarado
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Previene que la página sea embebida en un iframe (anti-clickjacking)
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Controla qué información del referrer se envía en las solicitudes
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Restringe el acceso a APIs del navegador (cámara, micrófono, geolocalización)
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // Content Security Policy: restringe orígenes de recursos permitidos
        // Scalar UI necesita cargar JS/CSS desde CDN, así que se relaja el CSP para esas rutas
        var isDocRoute = path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);

        if (isDocRoute)
        {
            // CSP permisivo para la documentación de API (solo disponible en Development)
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self' https://cdn.jsdelivr.net; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data: https:; " +
                "frame-ancestors 'none'";
        }
        else
        {
            // CSP estricto para el resto de la aplicación
            // frame-ancestors 'none' refuerza X-Frame-Options en navegadores modernos
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'";
        }

        // ── Cache-Control para endpoints sensibles ──
        // Evita que proxies/navegadores cacheen respuestas con datos de autenticación
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers["Cache-Control"] = "no-store";
            context.Response.Headers["Pragma"] = "no-cache";
        }

        await _next(context);
    }
}

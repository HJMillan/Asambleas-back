using Serilog.Context;

namespace Asambleas.Infrastructure.Middleware;

/// <summary>
/// Genera o lee un Correlation ID por request para trazabilidad distribuida.
/// Si el cliente envía el header X-Correlation-Id, se reutiliza; si no, se genera uno nuevo.
/// El ID se agrega al LogContext de Serilog y al response header.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Leer del request header o generar uno nuevo
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");

        // Agregar al response para que el cliente pueda correlacionar
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Pushear al LogContext de Serilog para que todos los logs del request incluyan este ID
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

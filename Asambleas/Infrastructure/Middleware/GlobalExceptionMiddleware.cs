using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Asambleas.Infrastructure.Middleware;

/// <summary>
/// Middleware global de excepciones que retorna respuestas ProblemDetails (RFC 7807).
///
/// Decisiones de seguridad:
/// - NUNCA expone stack traces, mensajes internos ni detalles de BD al cliente.
/// - ArgumentException/InvalidOperationException devuelven mensajes genéricos
///   (los mensajes detallados se logean internamente).
/// - Solo excepciones explícitamente mapeadas retornan su mensaje;
///   todo lo demás es "Error interno del servidor".
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no manejada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail) = exception switch
        {
            // 401 - Credenciales inválidas, token expirado, etc.
            // Se devuelve el mensaje porque AuthService ya controla qué texto exponer.
            UnauthorizedAccessException uae =>
                ((int)HttpStatusCode.Unauthorized, "No autorizado", uae.Message),

            // 400 - Errores de validación de entrada.
            // Mensaje genérico: el detalle queda en los logs.
            ArgumentException =>
                ((int)HttpStatusCode.BadRequest, "Solicitud inválida",
                 "Los datos proporcionados no son válidos."),

            // 404 - Recurso no encontrado.
            KeyNotFoundException =>
                ((int)HttpStatusCode.NotFound, "Recurso no encontrado",
                 "El recurso solicitado no existe."),

            // 409 - Conflicto de negocio (ej: CUIL duplicado).
            // Mensaje genérico para no filtrar información de existencia de registros.
            InvalidOperationException =>
                ((int)HttpStatusCode.Conflict, "Conflicto",
                 "No se pudo completar la operación. Verifique los datos e intente nuevamente."),

            // 500 - Todo lo demás: NUNCA exponer detalles internos.
            _ => ((int)HttpStatusCode.InternalServerError, "Error interno del servidor",
                  "Ocurrió un error inesperado. Por favor intente más tarde.")
        };

        context.Response.StatusCode = statusCode;

        // Usar el tipo ProblemDetails estándar de ASP.NET (RFC 7807)
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        // Agregar correlation ID si existe en el response header
        if (context.Response.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId.ToString();
        }

        // Incluir traceId para correlación con logs del servidor
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

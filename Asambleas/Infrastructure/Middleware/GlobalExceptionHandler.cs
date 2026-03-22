using Asambleas.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Asambleas.Infrastructure.Middleware;

/// <summary>
/// H34: Global exception handler usando IExceptionHandler (recomendado desde .NET 8).
/// Reemplaza el middleware custom por el mecanismo nativo del framework.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, "No autorizado", ex.Message),
            ForbiddenException ex => (StatusCodes.Status403Forbidden, "Acceso denegado", ex.Message),
            EntityNotFoundException ex => (StatusCodes.Status404NotFound, "No encontrado", ex.Message),
            DuplicateEntityException ex => (StatusCodes.Status409Conflict, "Entidad duplicada", ex.Message),
            BusinessRuleException ex => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio", ex.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida", "Los datos proporcionados no son válidos."),
            FluentValidation.ValidationException ex => (StatusCodes.Status400BadRequest, "Error de validación",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
        };

        // Solo loguear como Error los 5xx; los 4xx son errores esperados
        if (statusCode >= 500)
            logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        else
            logger.LogWarning("Error de cliente {StatusCode}: {Message}", statusCode, exception.Message);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier,
                ["correlationId"] = httpContext.Items.TryGetValue("CorrelationId", out var cid) ? cid?.ToString() : null
            }
        }, ct);

        return true;
    }
}

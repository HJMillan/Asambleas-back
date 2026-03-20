using System.Net;
using System.Text.Json;

namespace Asambleas.Infrastructure.Middleware;

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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "No autorizado."),
            ArgumentException argEx => ((int)HttpStatusCode.BadRequest, argEx.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Recurso no encontrado."),
            InvalidOperationException opEx => ((int)HttpStatusCode.Conflict, opEx.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = ReasonPhrase(statusCode),
            status = statusCode,
            detail = message,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Internal Server Error"
    };
}

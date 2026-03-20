using System.Text;
using System.Threading.RateLimiting;
using Asambleas.Features.Auth;
using Asambleas.Infrastructure.Database;
using Asambleas.Infrastructure.Middleware;
using Asambleas.Infrastructure.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// ══════════════════════════════════════════════════════
// BOOTSTRAP: Serilog temprano para capturar errores de startup
// ══════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ══════════════════════════════════════════════════
    // SERILOG: Reemplaza el logging por defecto
    // Sink: Console (dev) + File con rotación diaria (audit trail)
    // Enrich: CorrelationId se agrega vía CorrelationIdMiddleware
    // ══════════════════════════════════════════════════
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Application", "AsambleasAPI")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:l} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/asambleas-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    );

    // ── Kestrel: Limitar tamaño máximo del body a 1 MB ──
    // Previene ataques de denegación de servicio con payloads gigantes
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 1_048_576; // 1 MB
    });

    // ── Database ──
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ── JWT Settings ──
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.AddSingleton<JwtTokenService>();

    // ══════════════════════════════════════════════════
    // AUTHENTICATION: JWT Bearer con lectura desde cookies HTTP-only
    // ══════════════════════════════════════════════════
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero // Sin margen de gracia; el refresh token cubre la transición
        };

        // Leer JWT desde cookie HTTP-only (no desde header Authorization)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

    // ══════════════════════════════════════════════════
    // AUTHORIZATION: Políticas basadas en roles
    // RequireAuthorization global se aplica en MapControllers más abajo
    // ══════════════════════════════════════════════════
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("AdminOnly", policy =>
            policy.RequireRole("ADMIN_SISTEMA"))
        .AddPolicy("OperadorPlus", policy =>
            policy.RequireRole("OPERADOR", "ADMIN_SISTEMA", "TRIBUNAL"))
        .AddPolicy("Auditor", policy =>
            policy.RequireRole("AUDITOR", "ADMIN_SISTEMA"));

    // ── Services (DI) ──
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<ILdapService, LdapServiceStub>();
    builder.Services.AddScoped<IDomelecService, DomelecServiceStub>();

    // ══════════════════════════════════════════════════
    // FLUENT VALIDATION: Validación automática de DTOs
    // Los validators se registran desde el assembly actual
    // ══════════════════════════════════════════════════
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ══════════════════════════════════════════════════
    // CORS: Orígenes permitidos desde configuración
    // NUNCA usar AllowAnyOrigin en producción
    // ══════════════════════════════════════════════════
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            var origins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ══════════════════════════════════════════════════
    // RATE LIMITING: Protección contra fuerza bruta
    //
    // "auth": Fixed window, 5 req/min por IP → para login, register, refresh
    // "api":  Sliding window, 100 req/min por IP → para todos los demás endpoints
    // ══════════════════════════════════════════════════
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Política estricta para endpoints de autenticación
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0; // Sin cola: rechazar inmediatamente
        });

        // Política general para el resto de la API
        options.AddSlidingWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.SegmentsPerWindow = 4; // 4 segmentos de 15 segundos
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        // Handler personalizado para 429 con ProblemDetails
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/problem+json";

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString();
            }

            var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = "https://httpstatuses.com/429",
                Title = "Demasiadas solicitudes",
                Status = StatusCodes.Status429TooManyRequests,
                Detail = "Ha excedido el límite de solicitudes. Intente nuevamente más tarde.",
                Instance = context.HttpContext.Request.Path
            };

            await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        };
    });

    // ── Controllers + Swagger ──
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Asambleas API",
            Version = "v1",
            Description = "API Backend para el sistema de Asambleas Educativas"
        });
    });

    var app = builder.Build();

    // ══════════════════════════════════════════════════
    // MIDDLEWARE PIPELINE (orden importa)
    // ══════════════════════════════════════════════════

    // 1. Correlation ID (primero, para que todos los logs del pipeline lo tengan)
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Exception handler (captura todo lo que falle abajo en el pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 3. Security headers (en todas las respuestas)
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // 4. Serilog HTTP request logging (reemplaza el logging por defecto de ASP.NET)
    app.UseSerilogRequestLogging(options =>
    {
        // No loguear datos sensibles: excluir body y headers de auth
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    // 5. Swagger (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Asambleas API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // 6. HTTPS + HSTS (forzar HTTPS en producción)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts(); // Strict-Transport-Security header
    }
    app.UseHttpsRedirection();

    // 7. Rate limiting
    app.UseRateLimiter();

    // 8. CORS
    app.UseCors("FrontendPolicy");

    // 9. Auth
    app.UseAuthentication();
    app.UseAuthorization();

    // 10. Map controllers con autorización global por defecto.
    // Todos los endpoints requieren autenticación EXCEPTO los marcados con [AllowAnonymous].
    // Esto es "secure by default": olvidarse de poner [Authorize] no deja un endpoint abierto.
    app.MapControllers()
       .RequireAuthorization()
       .RequireRateLimiting("api"); // Rate limiting general para toda la API

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

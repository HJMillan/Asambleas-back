using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.ResponseCompression;
using System.Text;
using System.Threading.RateLimiting;
using Asambleas.Features.Auth;
using Asambleas.Infrastructure.Database;
using Asambleas.Infrastructure.HealthChecks;
using Asambleas.Infrastructure.Middleware;
using Asambleas.Infrastructure.Security;
using Asambleas.Infrastructure.Seeding;
using Asambleas.Infrastructure.Jobs;
using Asambleas.Infrastructure.Filters;
using FluentValidation;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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

    // ── HttpContextAccessor (#6: necesario para AuditInterceptor) ──
    builder.Services.AddHttpContextAccessor();

    // ── Database ──
    // AuditInterceptor se inyecta vía DI (Scoped) para acceder a IHttpContextAccessor
    builder.Services.AddScoped<AuditInterceptor>();
    builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null))
        .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

    // ── JWT Settings (con validación fail-fast al startup) ──
    builder.Services.AddOptions<JwtSettings>()
        .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
        .Validate(s => !string.IsNullOrEmpty(s.Secret) && s.Secret.Length >= 32,
            "JWT Secret debe tener al menos 32 caracteres y no estar vacío.")
        .Validate(s => !s.Secret.Contains("CAMBIAR") && !s.Secret.Contains("SET_VIA"),
            "JWT Secret tiene un valor placeholder. Configurar un secret real.")
        .Validate(s => s.AccessTokenExpirationMinutes > 0,
            "AccessTokenExpirationMinutes debe ser positivo.")
        .ValidateOnStart();
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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
            policy.RequireRole("OPERADOR", "ADMIN_SISTEMA"))
        .AddPolicy("Auditor", policy =>
            policy.RequireRole("AUDITOR", "ADMIN_SISTEMA"));

    // ── Repositories (#2: inversión de dependencias) ──
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<Asambleas.Features.Asambleas.IAsambleaRepository, AsambleaRepository>();
    builder.Services.AddScoped<Asambleas.Features.Vacantes.IVacanteRepository, VacanteRepository>();

    // ── Unit of Work (resuelve desde ApplicationDbContext registrado arriba) ──
    builder.Services.AddScoped<Asambleas.Common.IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // ── Services (DI) ──
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<Asambleas.Features.Usuarios.IUsuariosService, Asambleas.Features.Usuarios.UsuariosService>();
    builder.Services.AddScoped<Asambleas.Features.Asambleas.IAsambleasService, Asambleas.Features.Asambleas.AsambleasService>();
    builder.Services.AddScoped<Asambleas.Features.Vacantes.IVacantesService, Asambleas.Features.Vacantes.VacantesService>();
    builder.Services.AddScoped<ILdapService, LdapServiceStub>();
    builder.Services.AddScoped<IDomelecService, DomelecServiceStub>();
    builder.Services.AddHostedService<TokenCleanupService>();

    // ── Resilient HttpClients (#8/#10: retry + circuit breaker + timeouts) ──
    // Configurados para cuando se reemplacen los stubs por implementaciones reales.
    // AddStandardResilienceHandler agrega: retry, circuit breaker, total request timeout, attempt timeout, rate limiter.
    builder.Services.AddHttpClient("LdapClient", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ExternalServices:LdapUrl"] ?? "https://ldap.example.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    }).AddStandardResilienceHandler();

    builder.Services.AddHttpClient("DomelecClient", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ExternalServices:DomelecUrl"] ?? "https://domelec.example.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    }).AddStandardResilienceHandler();

    // ── HttpClient para SeqHealthCheck (evita socket exhaustion) ──
    builder.Services.AddHttpClient("SeqHealthCheck");

    // ── Health Checks (#9: DB + servicios externos) ──
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql", tags: ["db", "ready"])
        .AddCheck<LdapHealthCheck>("ldap", tags: ["external", "ready"])
        .AddCheck<DomelecHealthCheck>("domelec", tags: ["external", "ready"])
        .AddCheck<SeqHealthCheck>("seq", tags: ["logging"]);

    // ── FluentValidation (sin paquete deprecado AspNetCore) ──
    // ADR-001: Se decidió NO usar MediatR/CQRS. ValidationFilter actúa como pipeline behavior.
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ── Exception Handler (IExceptionHandler nativo) ──
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── ADR-002: Anti-Forgery removido ──
    // CSRF protection se logra vía cookies SameSite=Strict.
    // Ver docs/adr/ADR-002-antiforgery-samesite-strategy.md

    // ── Output Cache ──
    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("short", p => p.Expire(TimeSpan.FromSeconds(30)));
        options.AddPolicy("medium", p => p.Expire(TimeSpan.FromMinutes(5)));
    });
    builder.Services.AddMemoryCache();

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
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
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

    // ── Response Compression ──
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

    // ── Controllers + OpenAPI (Scalar reemplaza Swashbuckle) ──
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>(); // Reemplaza auto-validación deprecada
    });
    builder.Services.AddOpenApi();

    // ── API Versioning ──
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ── OpenTelemetry (Tracing + Metrics) ──
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter());

    // ── Graceful shutdown: dar tiempo a requests en progreso ──
    builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30));

    var app = builder.Build();

    // ══════════════════════════════════════════════════
    // AUTO-MIGRATE + SEED: Solo en Development
    // En producción, las migraciones se aplican manualmente durante el deploy
    // ══════════════════════════════════════════════════
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Migrate() solo funciona con proveedores relacionales (PostgreSQL, SQL Server, etc.)
        // InMemory (usado en tests de integración) usa EnsureCreated() en su lugar
        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
            Log.Information("Migraciones aplicadas automáticamente (Development)");
        }
        else
        {
            db.Database.EnsureCreated();
        }

        // #22: Seed data para admin inicial
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        await DbSeeder.SeedAdminAsync(db, logger, builder.Configuration);
    }

    // ══════════════════════════════════════════════════
    // MIDDLEWARE PIPELINE (orden importa)
    // ══════════════════════════════════════════════════

    // 1. Correlation ID (primero, para que todos los logs del pipeline lo tengan)
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 1b. Admin IP Whitelist (rechazar rápido antes de procesar)
    app.UseMiddleware<AdminIpWhitelistMiddleware>();

    // 2. Exception handler (IExceptionHandler nativo)
    app.UseExceptionHandler();

    // 3. Security headers (en todas las respuestas, ANTES del cache para incluirlos en respuestas cacheadas)
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // 4. Output Cache (después de security headers)
    app.UseOutputCache();

    // 5. Serilog HTTP request logging (reemplaza el logging por defecto de ASP.NET)
    app.UseSerilogRequestLogging(options =>
    {
        // No loguear datos sensibles: excluir body y headers de auth
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null) diagnosticContext.Set("UserId", userId);
        };
    });

    // 6. OpenAPI + Scalar (solo en desarrollo, reemplaza Swagger UI)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // 7. HTTPS + HSTS (forzar HTTPS en producción)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts(); // Strict-Transport-Security header
    }
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // 8. Response compression
    app.UseResponseCompression();

    // 9. CORS (ANTES de rate limiter para que preflight OPTIONS no sea rate-limited)
    app.UseCors("FrontendPolicy");

    // 10. Rate limiting (después de CORS)
    app.UseRateLimiter();

    // 11. Auth (ADR-002: Anti-Forgery removido, SameSite=Strict es suficiente)
    app.UseAuthentication();
    app.UseAuthorization();

    // 12. Map controllers con autorización global por defecto.
    // Todos los endpoints requieren autenticación EXCEPTO los marcados con [AllowAnonymous].
    // Esto es "secure by default": olvidarse de poner [Authorize] no deja un endpoint abierto.
    app.MapControllers()
       .RequireAuthorization()
       .RequireRateLimiting("api");

    // Health check endpoints (sin auth ni rate limiting)
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    }).AllowAnonymous();
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    }).AllowAnonymous();

    // Graceful shutdown
    app.Lifetime.ApplicationStopping.Register(() =>
        Log.Information("Aplicación cerrándose... esperando requests en progreso."));

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

/// <summary>
/// Entry point marker para WebApplicationFactory en tests de integración.
/// </summary>
public partial class Program { }

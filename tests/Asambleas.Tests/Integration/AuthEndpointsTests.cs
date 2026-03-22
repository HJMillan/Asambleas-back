using System.Net;
using System.Net.Http.Json;
using Asambleas.Features.Auth;
using Asambleas.Infrastructure.Database;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// HttpStatusCode.UnprocessableEntity = 422 (since .NET 6)

namespace Asambleas.Tests.Integration;

/// <summary>
/// Tests de integración para los endpoints de autenticación.
/// Usa WebApplicationFactory para levantar la app real con InMemory DB.
/// </summary>
public class AuthEndpointsTests : IClassFixture<AuthEndpointsTests.CustomFactory>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Factory personalizada que reemplaza PostgreSQL con InMemory DB.
    /// </summary>
    public class CustomFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Proveer configuración JWT válida para que la validación de opciones pase
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:Secret"] = "SuperSecretTestKeyThatIsAtLeast32CharactersLong!!",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience",
                    ["JwtSettings:AccessTokenExpirationMinutes"] = "15",
                    ["JwtSettings:RefreshTokenExpirationDays"] = "7"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Remover TODAS las registraciones relacionadas con EF Core
                // ConfigureTestServices se ejecuta DESPUÉS de Program.cs,
                // así que podemos remover las registraciones de Npgsql correctamente
                var efDescriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(ApplicationDbContext) ||
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                        d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true ||
                        d.ImplementationType?.FullName?.Contains("Npgsql") == true)
                    .ToList();
                foreach (var d in efDescriptors) services.Remove(d);

                // Remover AuditInterceptor ya que no se necesita en tests
                var auditDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("AuditInterceptor") == true ||
                                d.ImplementationType?.FullName?.Contains("AuditInterceptor") == true)
                    .ToList();
                foreach (var d in auditDescriptors) services.Remove(d);

                // Remover TODOS los health checks que refieran a Npgsql o PostgreSQL (service descriptors)
                var healthCheckDescriptors = services
                    .Where(d => d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                                d.ImplementationType?.FullName?.Contains("NpgSql") == true)
                    .ToList();
                foreach (var d in healthCheckDescriptors) services.Remove(d);

                // Remover el health check "postgresql" registrado via IHealthChecksBuilder
                services.PostConfigure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>(opts =>
                {
                    var toRemove = opts.Registrations.Where(r => r.Name == "postgresql").ToList();
                    foreach (var r in toRemove) opts.Registrations.Remove(r);
                });

                // Registrar InMemory DB con supresión de warnings de transacciones
                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.ConfigureWarnings(w =>
                        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        }
    }

    public AuthEndpointsTests(CustomFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns200()
    {
        var content = JsonContent.Create(new
        {
            Cuil = "20345678901",
            Nombre = "Test",
            Apellido = "User",
            Email = "test@test.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        var response = await _client.PostAsync("/api/v1/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.User.Cuil.Should().Be("20345678901");
        body.Message.Should().Contain("exitoso");
    }

    [Fact]
    public async Task Register_InvalidCuil_Returns400()
    {
        var content = JsonContent.Create(new
        {
            Cuil = "123",
            Nombre = "Test",
            Apellido = "User",
            Email = "test2@test.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        var response = await _client.PostAsync("/api/v1/auth/register", content);
        // ValidationFilter ahora retorna 422 (Unprocessable Entity) en vez de 400
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WrongCredentials_Returns401()
    {
        var content = JsonContent.Create(new
        {
            Cuil = "20345678901",
            Password = "WrongPassword1!"
        });

        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AfterRegister_Returns200()
    {
        // Register first
        var registerContent = JsonContent.Create(new
        {
            Cuil = "20345678902",
            Nombre = "Login",
            Apellido = "Test",
            Email = "login@test.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });
        var registerResponse = await _client.PostAsync("/api/v1/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login
        var loginContent = JsonContent.Create(new
        {
            Cuil = "20345678902",
            Password = "Password1!"
        });
        var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_WithoutTokens_Returns401()
    {
        var response = await _client.PostAsync("/api/v1/auth/refresh", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthReady_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FullAuthFlow_Register_SetsTokenCookies()
    {
        var content = JsonContent.Create(new
        {
            Cuil = "20345678903",
            Nombre = "Flow",
            Apellido = "Test",
            Email = "flow@test.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        var response = await _client.PostAsync("/api/v1/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que se setean cookies de tokens
        var setCookieHeaders = response.Headers
            .Where(h => h.Key == "Set-Cookie")
            .SelectMany(h => h.Value)
            .ToList();
        setCookieHeaders.Should().Contain(c => c.StartsWith("access_token="));
        setCookieHeaders.Should().Contain(c => c.StartsWith("refresh_token="));
    }
}

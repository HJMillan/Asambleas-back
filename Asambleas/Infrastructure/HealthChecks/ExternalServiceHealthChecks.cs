using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Asambleas.Infrastructure.HealthChecks;

/// <summary>
/// Health check para el servicio LDAP.
/// Verifica conectividad al directorio LDAP.
/// En stub mode, siempre retorna Healthy.
/// </summary>
public class LdapHealthCheck : IHealthCheck
{
    // TODO: Inyectar ILdapService o HttpClient cuando se implemente el servicio real
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Stub: siempre healthy hasta que se implemente LDAP real
        return Task.FromResult(HealthCheckResult.Healthy("LDAP service stub — no real connection"));
    }
}

/// <summary>
/// Health check para el servicio Domelec.
/// Verifica conectividad al endpoint de verificación de identidad.
/// En stub mode, siempre retorna Healthy.
/// </summary>
public class DomelecHealthCheck : IHealthCheck
{
    // TODO: Inyectar HttpClient configurado con resiliencia cuando se implemente
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Stub: siempre healthy hasta que se implemente Domelec real
        return Task.FromResult(HealthCheckResult.Healthy("Domelec service stub — no real connection"));
    }
}

/// <summary>
/// Health check para Seq (log aggregation).
/// Verifica que el servidor Seq está accesible.
/// Usa IHttpClientFactory para evitar socket exhaustion.
/// </summary>
public class SeqHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var seqUrl = configuration["Serilog:WriteTo:0:Args:serverUrl"];
        if (string.IsNullOrEmpty(seqUrl))
            return HealthCheckResult.Healthy("Seq no configurado — omitido");

        try
        {
            var httpClient = httpClientFactory.CreateClient("SeqHealthCheck");
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetAsync($"{seqUrl}/api", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Seq accesible en {seqUrl}")
                : HealthCheckResult.Degraded($"Seq respondió con {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Seq no accesible: {ex.Message}");
        }
    }
}

using Asambleas.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Jobs;

/// <summary>
/// Background service que limpia refresh tokens expirados y revocados cada 6 horas.
/// Previene crecimiento indefinido de la tabla refresh_tokens.
/// </summary>
/// <remarks>
/// Usa ExecuteDeleteAsync que elimina FÍSICAMENTE los registros,
/// no respeta el soft-delete de BaseEntity. Esto es INTENCIONAL:
/// los refresh tokens expirados/revocados no tienen valor de auditoría
/// y su eliminación permanente previene el crecimiento indefinido de la tabla.
/// La auditoría de tokens se cubre via AuditInterceptor al momento de la revocación.
/// </remarks>
public class TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Esperar un poco antes del primer ciclo para que la app arranque
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-30);
                var deleted = await db.RefreshTokens
                    .IgnoreQueryFilters() // Incluir soft-deleted si aplica
                    .Where(t => t.IsRevoked || t.ExpiresAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    logger.LogInformation(
                        "Token cleanup completado: {Count} tokens eliminados (revocados o expirados hace más de 30 días)",
                        deleted);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error durante limpieza de tokens");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}

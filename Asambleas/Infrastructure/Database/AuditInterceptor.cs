using System.Security.Claims;
using System.Text.Json;
using Asambleas.Common.Entities;
using Asambleas.Features.Listados.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// Interceptor que genera entradas de auditoría automáticamente
/// al crear, modificar o eliminar entidades. Registra OldValues, NewValues,
/// UserId e IpAddress del request actual para trazabilidad completa.
/// </summary>
public class AuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepta SaveChanges para generar audit log entries.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not ApplicationDbContext db)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Obtener info del usuario actual desde HttpContext
        var httpContext = httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = httpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        var auditEntries = new List<AuditLogEntry>();

        foreach (var entry in db.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            // No auditar la propia tabla de audit
            if (entry.Entity is AuditLogEntry) continue;

            var action = entry.State switch
            {
                EntityState.Added => TipoAccionAudit.CREACION,
                // Soft-delete: ApplyConventions() convierte Delete→Modified con IsDeleted=true
                EntityState.Modified when entry.Entity.IsDeleted
                    && entry.Property(nameof(BaseEntity.IsDeleted)).IsModified
                    => TipoAccionAudit.ELIMINACION,
                EntityState.Modified => TipoAccionAudit.MODIFICACION,
                EntityState.Deleted => TipoAccionAudit.ELIMINACION,
                _ => TipoAccionAudit.MODIFICACION
            };

            var auditEntry = new AuditLogEntry
            {
                Entidad = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id,
                Accion = action,
                Timestamp = DateTime.UtcNow,
                UserId = userId != null ? Guid.Parse(userId) : null,
                UserName = userName,
                IpAddress = ipAddress,
                OldValues = entry.State != EntityState.Added
                    ? SerializeProperties(entry.OriginalValues)
                    : null,
                NewValues = entry.State != EntityState.Deleted
                    ? SerializeProperties(entry.CurrentValues)
                    : null
            };

            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Count > 0)
        {
            db.AuditLog.AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string SerializeProperties(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in values.Properties)
        {
            // No serializar PasswordHash en audit logs por seguridad
            if (prop.Name == "PasswordHash") continue;
            dict[prop.Name] = values[prop];
        }
        return JsonSerializer.Serialize(dict);
    }
}

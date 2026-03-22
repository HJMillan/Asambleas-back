using Asambleas.Common.Entities;

namespace Asambleas.Features.Listados.Entities;

public enum TipoAccionAudit
{
    CREACION_ASAMBLEA,
    MODIFICACION_ASAMBLEA,
    PUBLICACION_VACANTE,
    INSCRIPCION_POSTULANTE,
    ADJUDICACION,
    RECHAZO_DESIGNACION,
    IMPUGNACION,
    RESOLUCION_IMPUGNACION,
    CAMBIO_ESTADO_VACANTE,
    CAMBIO_ROL_USUARIO,
    VERIFICACION_DOMELEC,
    // Genéricos (usados por AuditInterceptor)
    CREACION,
    MODIFICACION,
    ELIMINACION
}

public class AuditLogEntry : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public TipoAccionAudit Accion { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

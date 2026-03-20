using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Designaciones.Entities;

public enum EstadoDesignacion
{
    PENDIENTE,
    CONFIRMADA,
    RECHAZADA,
    IMPUGNADA,
    EN_REVISION,
    ANULADA,
    FINALIZADA,
    CANCELADA
}

public class Designacion : BaseEntity
{
    public int Instancia { get; set; } = 1; // 1, 2, 3
    public EstadoDesignacion Estado { get; set; } = EstadoDesignacion.PENDIENTE;
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public string? MotivoRechazo { get; set; }
    public string? CertificadoUrl { get; set; }

    // Foreign keys
    public Guid DocenteId { get; set; }
    public User Docente { get; set; } = null!;

    public Guid VacanteId { get; set; }
    public Vacantes.Entities.Vacante Vacante { get; set; } = null!;

    public Guid AsambleaId { get; set; }
    public Asambleas.Entities.Asamblea Asamblea { get; set; } = null!;

    // Navigation
    public ICollection<Impugnacion> Impugnaciones { get; set; } = [];
}

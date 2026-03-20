using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Designaciones.Entities;

public class Impugnacion : BaseEntity
{
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaLimite { get; set; } // Plazo 24hs
    public bool Resuelta { get; set; }
    public string? Resolucion { get; set; }
    public DateTime? FechaResolucion { get; set; }

    // Foreign keys
    public Guid DesignacionId { get; set; }
    public Designacion Designacion { get; set; } = null!;

    public Guid ReclamanteId { get; set; }
    public User Reclamante { get; set; } = null!;
}

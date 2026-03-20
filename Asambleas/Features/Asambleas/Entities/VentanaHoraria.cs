using Asambleas.Common.Entities;

namespace Asambleas.Features.Asambleas.Entities;

public class VentanaHoraria : BaseEntity
{
    public DateTime Inicio { get; set; }
    public DateTime Fin { get; set; }
    public bool Activa { get; set; } = true;

    // Foreign key
    public Guid AsambleaId { get; set; }
    public Asamblea Asamblea { get; set; } = null!;
}

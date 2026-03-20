using Asambleas.Common.Entities;
using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Vacantes.Entities;

public enum TipoCargo
{
    TITULAR,
    PROVISIONAL,
    INTERINO,
    SUPLENTE
}

public enum EstadoVacante
{
    PUBLICADA,
    EN_INSCRIPCION,
    EN_EVALUACION,
    EN_ADJUDICACION,
    ADJUDICADA,
    DESIERTA,
    CERRADA,
    CANCELADA
}

public enum Turno
{
    MANANA,
    TARDE,
    NOCHE,
    VESPERTINO
}

public class Vacante : BaseEntity
{
    public string Cargo { get; set; } = string.Empty;
    public NivelAsamblea Nivel { get; set; }
    public TipoCargo TipoCargo { get; set; }
    public EstadoVacante Estado { get; set; } = EstadoVacante.PUBLICADA;
    public int Modulos { get; set; }
    public int Horas { get; set; }
    public Turno Turno { get; set; }
    public string? Observaciones { get; set; }

    // Foreign keys
    public Guid EstablecimientoId { get; set; }
    public Establecimiento Establecimiento { get; set; } = null!;

    public Guid AsambleaId { get; set; }
    public Asamblea Asamblea { get; set; } = null!;
}

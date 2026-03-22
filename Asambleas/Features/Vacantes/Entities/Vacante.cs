using Asambleas.Common.Entities;
using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Vacantes.Entities;

/// <summary>
/// Tipo de cargo de una vacante docente.
/// </summary>
public enum TipoCargo
{
    TITULAR,
    PROVISIONAL,
    INTERINO,
    SUPLENTE
}

/// <summary>
/// Estado del ciclo de vida de una vacante.
/// </summary>
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

/// <summary>
/// Turno del cargo vacante.
/// </summary>
public enum Turno
{
    MANANA,
    TARDE,
    NOCHE,
    VESPERTINO
}

/// <summary>
/// Representa una vacante docente disponible para postulación en una asamblea.
/// Vinculada a un establecimiento y una asamblea específica.
/// </summary>
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

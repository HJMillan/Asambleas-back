using Asambleas.Common.Entities;

namespace Asambleas.Features.Asambleas.Entities;

/// <summary>
/// Nivel educativo de una asamblea.
/// </summary>
public enum NivelAsamblea
{
    INICIAL,
    PRIMARIO,
    SECUNDARIO,
    SUPERIOR,
    ESPECIAL
}

/// <summary>
/// Tipo de convocatoria de la asamblea.
/// </summary>
public enum TipoAsamblea
{
    ORDINARIA,
    EXTRAORDINARIA,
    COMPLEMENTARIA
}

/// <summary>
/// Estado del ciclo de vida de una asamblea.
/// </summary>
public enum EstadoAsamblea
{
    PROGRAMADA,
    EN_CURSO,
    FINALIZADA,
    CANCELADA
}

/// <summary>
/// Representa una asamblea de designación docente, con su fecha, nivel educativo,
/// ventanas de inscripción y horarios.
/// </summary>
public class Asamblea : BaseEntity
{
    public DateTime Fecha { get; set; }
    public NivelAsamblea Nivel { get; set; }
    public TipoAsamblea Tipo { get; set; }
    public EstadoAsamblea Estado { get; set; } = EstadoAsamblea.PROGRAMADA;

    public DateTime HorarioInicio { get; set; }
    public DateTime HorarioFin { get; set; }

    public DateTime VentanaInscripcionInicio { get; set; }
    public DateTime VentanaInscripcionFin { get; set; }

    public string Lugar { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // Navigation
    public ICollection<VentanaHoraria> VentanasHorarias { get; set; } = [];
}

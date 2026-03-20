using Asambleas.Common.Entities;

namespace Asambleas.Features.Asambleas.Entities;

public enum NivelAsamblea
{
    INICIAL,
    PRIMARIO,
    SECUNDARIO,
    SUPERIOR,
    ESPECIAL
}

public enum TipoAsamblea
{
    ORDINARIA,
    EXTRAORDINARIA,
    COMPLEMENTARIA
}

public enum EstadoAsamblea
{
    PROGRAMADA,
    EN_CURSO,
    FINALIZADA,
    CANCELADA
}

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

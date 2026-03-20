using Asambleas.Common.Entities;

namespace Asambleas.Features.DeclaracionJurada.Entities;

public enum SituacionRevista
{
    TITULAR,
    PROVISIONAL,
    INTERINO,
    SUPLENTE,
    CONTRATADO
}

public class EmpleoActual : BaseEntity
{
    public string Establecimiento { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public int Horas { get; set; }
    public SituacionRevista SituacionRevista { get; set; }
    public string? Jurisdiccion { get; set; }

    // Horarios L-S (almacenados como JSON o campos individuales)
    public string? HorarioLunes { get; set; }
    public string? HorarioMartes { get; set; }
    public string? HorarioMiercoles { get; set; }
    public string? HorarioJueves { get; set; }
    public string? HorarioViernes { get; set; }
    public string? HorarioSabado { get; set; }

    // Foreign key
    public Guid DeclaracionJuradaId { get; set; }
    public DeclaracionJurada DeclaracionJurada { get; set; } = null!;
}

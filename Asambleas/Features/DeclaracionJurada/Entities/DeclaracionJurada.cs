using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.DeclaracionJurada.Entities;

public enum PasoDeclaracion
{
    DATOS_PERSONALES,
    SITUACION_LABORAL,
    EMPLEOS_ACTUALES
}

public class DeclaracionJurada : BaseEntity
{
    public PasoDeclaracion PasoActual { get; set; } = PasoDeclaracion.DATOS_PERSONALES;
    public bool Completada { get; set; }
    public DateTime? FechaCompletado { get; set; }

    // Datos Personales
    public string Domicilio { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    // Situación Laboral
    public bool TrabajaEnOtraJurisdiccion { get; set; }
    public string? JurisdiccionActual { get; set; }
    public int HorasTotalesOcupadas { get; set; }

    // Foreign key
    public Guid DocenteId { get; set; }
    public User Docente { get; set; } = null!;

    // Navigation
    public ICollection<EmpleoActual> EmpleosActuales { get; set; } = [];
}

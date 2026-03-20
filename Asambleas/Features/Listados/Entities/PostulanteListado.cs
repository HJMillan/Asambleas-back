using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Listados.Entities;

public enum EstadoPostulante
{
    VIGENTE,
    INHABILITADO,
    PENALIZADO,
    AGOTADO
}

public class PostulanteListado : BaseEntity
{
    public decimal Puntaje { get; set; }
    public int Posicion { get; set; }
    public EstadoPostulante Estado { get; set; } = EstadoPostulante.VIGENTE;

    // Foreign keys
    public Guid DocenteId { get; set; }
    public User Docente { get; set; } = null!;

    public Guid ListadoOficialId { get; set; }
    public ListadoOficial ListadoOficial { get; set; } = null!;
}

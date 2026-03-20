using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Designaciones.Entities;

public class Postulacion : BaseEntity
{
    public int Posicion { get; set; }
    public decimal Puntaje { get; set; }

    // Foreign keys
    public Guid DocenteId { get; set; }
    public User Docente { get; set; } = null!;

    public Guid VacanteId { get; set; }
    public Vacantes.Entities.Vacante Vacante { get; set; } = null!;
}

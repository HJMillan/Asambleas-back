using Asambleas.Common.Entities;
using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Listados.Entities;

public enum TipoListado
{
    OFICIAL,
    COMPLEMENTARIO
}

public class ListadoOficial : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public NivelAsamblea Nivel { get; set; }
    public TipoListado Tipo { get; set; }
    public int Anio { get; set; }

    // Navigation
    public ICollection<PostulanteListado> Postulantes { get; set; } = [];
}

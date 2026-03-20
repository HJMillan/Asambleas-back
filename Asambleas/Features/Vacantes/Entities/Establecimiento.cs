using Asambleas.Common.Entities;

namespace Asambleas.Features.Vacantes.Entities;

public class Establecimiento : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string CodigoFuncional { get; set; } = string.Empty;

    // Navigation
    public ICollection<Vacante> Vacantes { get; set; } = [];
}

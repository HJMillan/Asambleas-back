using Asambleas.Common.Entities;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Docentes.Entities;

public enum NivelEducativo
{
    INICIAL,
    PRIMARIO,
    SECUNDARIO,
    SUPERIOR,
    ESPECIAL
}

public enum EstadoRevista
{
    TITULAR,
    PROVISIONAL,
    INTERINO,
    SUPLENTE,
    SIN_CARGO
}

public class DocenteProfile : BaseEntity
{
    public decimal Puntaje { get; set; }
    public NivelEducativo NivelEducativo { get; set; }
    public EstadoRevista EstadoRevista { get; set; } = EstadoRevista.SIN_CARGO;
    public bool Embarazo { get; set; }
    public bool DeclaracionJuradaPresentada { get; set; }
    public int CantidadRenuncias { get; set; }
    public bool Inhabilitado { get; set; }
    public string? MotivoInhabilitacion { get; set; }

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

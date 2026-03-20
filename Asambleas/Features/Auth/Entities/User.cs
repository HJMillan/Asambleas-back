using Asambleas.Common.Entities;
using Asambleas.Features.Docentes.Entities;

namespace Asambleas.Features.Auth.Entities;

public class User : BaseEntity
{
    public string Dni { get; set; } = string.Empty;
    public string Cuil { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.DOCENTE;
    public bool IsDomelecVerified { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // LDAP
    public string? LdapUsername { get; set; }

    // Navigation
    public DocenteProfile? DocenteProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

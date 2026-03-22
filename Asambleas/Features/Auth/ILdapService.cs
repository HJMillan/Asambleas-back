using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Interface for LDAP authentication.
/// Will be implemented with real LDAP later; uses a stub in development.
/// </summary>
public interface ILdapService
{
    /// <summary>
    /// Authenticates a user against the LDAP directory.
    /// Returns the user info if found, null if authentication fails.
    /// </summary>
    Task<LdapUserInfo?> AuthenticateAsync(string username, string password);
}

public record LdapUserInfo(
    string Username,
    string Nombre,
    string Apellido,
    string Email,
    string Dni,
    string Cuil
);

/// <summary>
/// Development stub that always returns success.
/// Replace with real LDAP implementation when server is available.
/// Genera datos únicos por username para evitar conflictos de unicidad en DB.
/// </summary>
public class LdapServiceStub : ILdapService
{
    public Task<LdapUserInfo?> AuthenticateAsync(string username, string password)
    {
        // Generar un hash numérico estable para el username
        var hash = Math.Abs(username.GetHashCode()) % 100000000;
        var dni = hash.ToString("D8");
        var cuil = $"20{dni}0";

        var mockUser = new LdapUserInfo(
            Username: username,
            Nombre: "Usuario",
            Apellido: username.ToUpperInvariant(),
            Email: $"{username}@municipio.gob.ar",
            Dni: dni,
            Cuil: cuil
        );

        return Task.FromResult<LdapUserInfo?>(mockUser);
    }
}

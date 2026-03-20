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
/// </summary>
public class LdapServiceStub : ILdapService
{
    public Task<LdapUserInfo?> AuthenticateAsync(string username, string password)
    {
        // Stub: always returns a mock user for development
        var mockUser = new LdapUserInfo(
            Username: username,
            Nombre: "Usuario",
            Apellido: "LDAP",
            Email: $"{username}@municipio.gob.ar",
            Dni: "12345678",
            Cuil: "20123456789"
        );

        return Task.FromResult<LdapUserInfo?>(mockUser);
    }
}

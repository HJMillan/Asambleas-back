using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Interface for Domelec verification service.
/// Will be implemented with external endpoint later; uses a stub in development.
/// </summary>
public interface IDomelecService
{
    /// <summary>
    /// Verifies a user's identity against the Domelec system using their CUIL.
    /// Returns true if verification succeeds.
    /// </summary>
    Task<bool> VerificarAsync(string cuil);
}

/// <summary>
/// Development stub that always returns verified.
/// Replace with real Domelec endpoint implementation later.
/// </summary>
public class DomelecServiceStub : IDomelecService
{
    public Task<bool> VerificarAsync(string cuil)
    {
        // Stub: always returns verified for development
        return Task.FromResult(true);
    }
}

using Asambleas.Common;
using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Asambleas;

/// <summary>
/// Interfaz del servicio de gestión de asambleas.
/// </summary>
public interface IAsambleasService
{
    /// <summary>
    /// Obtiene todas las asambleas con cantidad de vacantes.
    /// </summary>
    Task<List<AsambleaResponse>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Crea una nueva asamblea.
    /// </summary>
    Task<Result<Asamblea>> CreateAsync(CreateAsambleaRequest request, CancellationToken ct = default);
}

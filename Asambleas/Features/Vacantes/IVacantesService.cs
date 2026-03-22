using Asambleas.Common;
using Asambleas.Features.Vacantes.Entities;

namespace Asambleas.Features.Vacantes;

/// <summary>
/// Interfaz del servicio de gestión de vacantes.
/// </summary>
public interface IVacantesService
{
    /// <summary>
    /// Obtiene todas las vacantes con establecimiento.
    /// </summary>
    Task<List<VacanteResponse>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una vacante por ID.
    /// </summary>
    Task<Result<VacanteResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Crea una nueva vacante asociada a una asamblea.
    /// </summary>
    Task<Result<Vacante>> CreateAsync(CreateVacanteRequest request, CancellationToken ct = default);
}

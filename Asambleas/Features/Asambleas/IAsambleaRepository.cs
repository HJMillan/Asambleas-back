using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Asambleas;

/// <summary>
/// Abstracción de acceso a datos para asambleas.
/// </summary>
public interface IAsambleaRepository
{
    /// <summary>
    /// Obtiene todas las asambleas con la cantidad de vacantes asociadas (solo lectura).
    /// </summary>
    Task<List<(Asamblea Asamblea, int VacantesCount)>> GetAllWithVacantesCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una asamblea por ID (con tracking para updates).
    /// </summary>
    Task<Asamblea?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva asamblea.
    /// </summary>
    void Add(Asamblea asamblea);
}

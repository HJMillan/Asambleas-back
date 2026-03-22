using Asambleas.Features.Vacantes.Entities;

namespace Asambleas.Features.Vacantes;

/// <summary>
/// Abstracción de acceso a datos para vacantes.
/// </summary>
public interface IVacanteRepository
{
    /// <summary>
    /// Obtiene todas las vacantes con su establecimiento (solo lectura).
    /// </summary>
    Task<List<Vacante>> GetAllWithEstablecimientoAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una vacante por ID con su establecimiento (solo lectura).
    /// </summary>
    Task<Vacante?> GetByIdWithEstablecimientoAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Busca un establecimiento por su código funcional.
    /// </summary>
    Task<Establecimiento?> GetEstablecimientoByCodigoAsync(string codigoFuncional, CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva vacante.
    /// </summary>
    void Add(Vacante vacante);

    /// <summary>
    /// Agrega un nuevo establecimiento.
    /// </summary>
    void AddEstablecimiento(Establecimiento establecimiento);
}

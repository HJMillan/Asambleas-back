using Asambleas.Features.Vacantes;
using Asambleas.Features.Vacantes.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// Implementación de IVacanteRepository usando EF Core.
/// </summary>
public class VacanteRepository(ApplicationDbContext db) : IVacanteRepository
{
    public async Task<List<Vacante>> GetAllWithEstablecimientoAsync(CancellationToken ct = default)
        => await db.Vacantes
            .AsNoTracking()
            .Include(v => v.Establecimiento)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);

    public async Task<Vacante?> GetByIdWithEstablecimientoAsync(Guid id, CancellationToken ct = default)
        => await db.Vacantes
            .AsNoTracking()
            .Include(v => v.Establecimiento)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Establecimiento?> GetEstablecimientoByCodigoAsync(string codigoFuncional, CancellationToken ct = default)
        => await db.Establecimientos
            .FirstOrDefaultAsync(e => e.CodigoFuncional == codigoFuncional, ct);

    public void Add(Vacante vacante) => db.Vacantes.Add(vacante);

    public void AddEstablecimiento(Establecimiento establecimiento) => db.Establecimientos.Add(establecimiento);
}

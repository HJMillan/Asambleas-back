using Asambleas.Features.Asambleas;
using Asambleas.Features.Asambleas.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// Implementación de IAsambleaRepository usando EF Core.
/// </summary>
public class AsambleaRepository(ApplicationDbContext db) : IAsambleaRepository
{
    public async Task<List<(Asamblea Asamblea, int VacantesCount)>> GetAllWithVacantesCountAsync(CancellationToken ct = default)
    {
        var result = await db.Asambleas
            .AsNoTracking()
            .OrderByDescending(a => a.Fecha)
            .ThenBy(a => a.HorarioInicio)
            .Select(a => new
            {
                Asamblea = a,
                VacantesCount = db.Vacantes.Count(v => v.AsambleaId == a.Id)
            })
            .ToListAsync(ct);

        return result.Select(x => (x.Asamblea, x.VacantesCount)).ToList();
    }

    public async Task<Asamblea?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Asambleas.FirstOrDefaultAsync(a => a.Id == id, ct);

    public void Add(Asamblea asamblea) => db.Asambleas.Add(asamblea);
}

using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// Implementación de IUserRepository usando EF Core.
/// </summary>
public class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public async Task<User?> GetByCuilAsync(string cuil, CancellationToken ct = default)
        => await CompiledQueries.GetUserByCuil(db, cuil);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await CompiledQueries.GetUserByEmail(db, email);

    public async Task<User?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default)
        => await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default)
        => await db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByLdapUsernameAsync(string username, CancellationToken ct = default)
        => await CompiledQueries.GetUserByLdapUsername(db, username);

    public async Task<bool> ExistsByCuilAsync(string cuil, CancellationToken ct = default)
        => await CompiledQueries.ExistsUserByCuil(db, cuil);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await CompiledQueries.ExistsUserByEmail(db, email);

    public async Task<List<User>> GetAllReadOnlyAsync(CancellationToken ct = default)
        => await db.Users.AsNoTracking().OrderBy(u => u.Apellido).ThenBy(u => u.Nombre).ToListAsync(ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public void Add(User user) => db.Users.Add(user);
}

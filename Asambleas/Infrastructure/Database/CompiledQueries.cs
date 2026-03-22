using Asambleas.Features.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asambleas.Infrastructure.Database;

/// <summary>
/// H21: Compiled Queries para las consultas más frecuentes.
/// Elimina el overhead de compilación de LINQ en cada ejecución.
/// NOTA: Las compiled queries NO soportan Include/ThenInclude.
/// Cuando se necesiten navigation properties (ej: DocenteProfile, RefreshTokens),
/// usar queries estándar (ver UserRepository.GetByIdWithTokensAsync).
/// </summary>
public static class CompiledQueries
{
    public static readonly Func<ApplicationDbContext, string, Task<User?>> GetUserByCuil =
        EF.CompileAsyncQuery((ApplicationDbContext db, string cuil) =>
            db.Users.FirstOrDefault(u => u.Cuil == cuil));

    public static readonly Func<ApplicationDbContext, Guid, Task<User?>> GetUserById =
        EF.CompileAsyncQuery((ApplicationDbContext db, Guid id) =>
            db.Users.FirstOrDefault(u => u.Id == id));

    public static readonly Func<ApplicationDbContext, string, Task<User?>> GetUserByEmail =
        EF.CompileAsyncQuery((ApplicationDbContext db, string email) =>
            db.Users.FirstOrDefault(u => u.Email == email));

    public static readonly Func<ApplicationDbContext, string, Task<User?>> GetUserByLdapUsername =
        EF.CompileAsyncQuery((ApplicationDbContext db, string username) =>
            db.Users.FirstOrDefault(u => u.LdapUsername == username));

    public static readonly Func<ApplicationDbContext, string, Task<bool>> ExistsUserByCuil =
        EF.CompileAsyncQuery((ApplicationDbContext db, string cuil) =>
            db.Users.Any(u => u.Cuil == cuil));

    public static readonly Func<ApplicationDbContext, string, Task<bool>> ExistsUserByEmail =
        EF.CompileAsyncQuery((ApplicationDbContext db, string email) =>
            db.Users.Any(u => u.Email == email));

    public static readonly Func<ApplicationDbContext, string, Task<RefreshToken?>> GetRefreshToken =
        EF.CompileAsyncQuery((ApplicationDbContext db, string token) =>
            db.RefreshTokens.FirstOrDefault(t => t.Token == token));
}

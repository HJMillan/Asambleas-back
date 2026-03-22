using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Abstracción de acceso a datos para usuarios.
/// Permite testabilidad sin depender de EF Core directamente.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Busca un usuario por CUIL. Retorna null si no existe.
    /// </summary>
    Task<User?> GetByCuilAsync(string cuil, CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por email. Retorna null si no existe.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por ID sin tracking (solo lectura).
    /// </summary>
    Task<User?> GetByIdReadOnlyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por ID con tracking e incluye RefreshTokens.
    /// </summary>
    Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por username LDAP.
    /// </summary>
    Task<User?> GetByLdapUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un usuario con el CUIL dado.
    /// </summary>
    Task<bool> ExistsByCuilAsync(string cuil, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un usuario con el email dado.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios sin tracking (solo lectura).
    /// </summary>
    Task<List<User>> GetAllReadOnlyAsync(CancellationToken ct = default);

    /// <summary>
    /// Busca un usuario por ID con tracking (para actualizaciones).
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo usuario al contexto.
    /// </summary>
    void Add(User user);
}

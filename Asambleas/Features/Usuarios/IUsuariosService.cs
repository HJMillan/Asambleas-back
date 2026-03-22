using Asambleas.Common;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Usuarios;

/// <summary>
/// Interfaz del servicio de gestión de usuarios para admin.
/// </summary>
public interface IUsuariosService
{
    /// <summary>
    /// Obtiene todos los usuarios (solo lectura).
    /// </summary>
    Task<List<User>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Actualiza el rol de un usuario.
    /// </summary>
    Task<Result<User>> UpdateRoleAsync(Guid userId, Role newRole, CancellationToken ct = default);
}

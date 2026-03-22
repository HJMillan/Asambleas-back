using Asambleas.Features.Auth;

namespace Asambleas.Features.Usuarios;

// ── Requests ──

/// <summary>
/// Request para actualizar el rol de un usuario.
/// </summary>
/// <param name="Role">Nombre del rol (debe coincidir con el enum Role).</param>
public record UpdateUserRoleRequest(string Role);

// ── Responses ──

/// <summary>
/// Respuesta tras actualizar el rol de un usuario.
/// </summary>
/// <param name="User">Datos públicos del usuario actualizado.</param>
/// <param name="Message">Mensaje descriptivo.</param>
public record UpdateUserRoleResponse(UserInfoResponse User, string Message);

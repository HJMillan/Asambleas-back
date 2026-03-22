using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Auth;

/// <summary>
/// Extensiones de mapeo de User a DTOs de respuesta.
/// Centraliza la lógica de mapeo que antes estaba en AuthService.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convierte una entidad User a un DTO de respuesta pública.
    /// </summary>
    public static UserInfoResponse ToResponse(this User user) => new(
        Id: user.Id,
        Dni: user.Dni,
        Cuil: user.Cuil,
        Nombre: user.Nombre,
        Apellido: user.Apellido,
        Email: user.Email,
        Role: user.Role.ToString(),
        IsDomelecVerified: user.IsDomelecVerified
    );
}

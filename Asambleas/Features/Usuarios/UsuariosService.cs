using Asambleas.Common;
using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Entities;

namespace Asambleas.Features.Usuarios;

/// <summary>
/// Servicio de gestión de usuarios (admin). Usa IUserRepository + IUnitOfWork.
/// </summary>
public class UsuariosService : IUsuariosService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsuariosService> _logger;

    public UsuariosService(IUserRepository users, IUnitOfWork unitOfWork, ILogger<UsuariosService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _users.GetAllReadOnlyAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Result<User>> UpdateRoleAsync(Guid userId, Role newRole, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);

        if (user is null)
            return Result<User>.NotFound("Usuario no encontrado.");

        var previousRole = user.Role;

        if (previousRole == newRole)
            return Result<User>.Failure($"El usuario ya tiene el rol {newRole}.");

        user.Role = newRole;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Rol actualizado. UserId={UserId}, PreviousRole={Previous}, NewRole={New}",
            userId, previousRole, newRole);

        return Result<User>.Success(user);
    }
}

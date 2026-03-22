using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asambleas.Features.Usuarios;

/// <summary>
/// Controller de gestión de usuarios. Solo accesible para ADMIN_SISTEMA.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/usuarios")]
[Authorize(Policy = "AdminOnly")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _service;

    public UsuariosController(IUsuariosService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios con datos públicos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserInfoResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _service.GetAllAsync(ct);
        var response = users.Select(u => u.ToResponse()).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Actualiza el rol de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario a modificar.</param>
    /// <param name="request">Nuevo rol.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Datos actualizados del usuario y mensaje de confirmación.</returns>
    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(UpdateUserRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateUserRoleResponse>> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        if (!Enum.TryParse<Role>(request.Role, ignoreCase: false, out var newRole))
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Solicitud inválida",
                Status = 400,
                Detail = $"Rol inválido: '{request.Role}'. Valores permitidos: {string.Join(", ", Enum.GetNames<Role>())}"
            });
        }

        var result = await _service.UpdateRoleAsync(id, newRole, ct);

        return result.Match<ActionResult<UpdateUserRoleResponse>>(
            onSuccess: user => Ok(new UpdateUserRoleResponse(
                User: user.ToResponse(),
                Message: $"Rol actualizado a {user.Role} correctamente."
            )),
            onFailure: error => result.StatusCode switch
            {
                404 => NotFound(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "No encontrado",
                    Status = 404,
                    Detail = error
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Solicitud inválida",
                    Status = 400,
                    Detail = error
                })
            }
        );
    }
}

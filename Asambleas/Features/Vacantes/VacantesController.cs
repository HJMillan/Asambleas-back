using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asambleas.Features.Vacantes;

/// <summary>
/// Controller de vacantes. GET para todos los autenticados, POST para OperadorPlus.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/vacantes")]
[Authorize]
public class VacantesController : ControllerBase
{
    private readonly IVacantesService _service;

    public VacantesController(IVacantesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene todas las vacantes con sus establecimientos.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VacanteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VacanteResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una vacante por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VacanteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacanteResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);

        return result.Match<ActionResult<VacanteResponse>>(
            onSuccess: v => Ok(v),
            onFailure: error => NotFound(new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "No encontrado",
                Status = 404,
                Detail = error
            })
        );
    }

    /// <summary>
    /// Crea una nueva vacante asociada a una asamblea. Solo OperadorPlus.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OperadorPlus")]
    [ProducesResponseType(typeof(VacanteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacanteResponse>> Create(
        [FromBody] CreateVacanteRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);

        return result.Match<ActionResult<VacanteResponse>>(
            onSuccess: vacante =>
            {
                var response = vacante.ToResponse();
                return CreatedAtAction(nameof(GetById), new { id = vacante.Id }, response);
            },
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

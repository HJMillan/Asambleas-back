using Asambleas.Features.Asambleas.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asambleas.Features.Asambleas;

/// <summary>
/// Controller de asambleas. GET para todos los autenticados, POST para OperadorPlus.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/asambleas")]
[Authorize]
public class AsambleasController : ControllerBase
{
    private readonly IAsambleasService _service;

    public AsambleasController(IAsambleasService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene todas las asambleas con cantidad de vacantes.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AsambleaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AsambleaResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva asamblea. Solo OperadorPlus.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OperadorPlus")]
    [ProducesResponseType(typeof(AsambleaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AsambleaResponse>> Create(
        [FromBody] CreateAsambleaRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);

        return result.Match<ActionResult<AsambleaResponse>>(
            onSuccess: asamblea =>
            {
                var response = asamblea.ToResponse();
                return CreatedAtAction(nameof(GetAll), new { id = asamblea.Id }, response);
            },
            onFailure: error => BadRequest(new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Solicitud inválida",
                Status = 400,
                Detail = error
            })
        );
    }
}

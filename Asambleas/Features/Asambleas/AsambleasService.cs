using Asambleas.Common;
using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Asambleas;

/// <summary>
/// Servicio de gestión de asambleas.
/// </summary>
public class AsambleasService : IAsambleasService
{
    private readonly IAsambleaRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AsambleasService> _logger;

    public AsambleasService(IAsambleaRepository repo, IUnitOfWork unitOfWork, ILogger<AsambleasService> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<AsambleaResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllWithVacantesCountAsync(ct);
        return items.Select(x => x.Asamblea.ToResponse(x.VacantesCount)).ToList();
    }

    /// <inheritdoc />
    public async Task<Result<Asamblea>> CreateAsync(CreateAsambleaRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<NivelAsamblea>(request.Nivel, ignoreCase: true, out var nivel))
            return Result<Asamblea>.Failure($"Nivel inválido: '{request.Nivel}'.");

        if (!Enum.TryParse<TipoAsamblea>(request.Tipo, ignoreCase: true, out var tipo))
            return Result<Asamblea>.Failure($"Tipo inválido: '{request.Tipo}'.");

        if (!DateOnly.TryParse(request.Fecha, out var fecha))
            return Result<Asamblea>.Failure("Formato de fecha inválido.");

        if (!TimeOnly.TryParse(request.HoraInicio, out var horaInicio))
            return Result<Asamblea>.Failure("Formato de hora de inicio inválido.");

        if (!TimeOnly.TryParse(request.HoraFin, out var horaFin))
            return Result<Asamblea>.Failure("Formato de hora de fin inválido.");

        if (!DateTime.TryParse(request.VentanaInscripcionInicio, out var ventanaInicio))
            return Result<Asamblea>.Failure("Formato de inicio de ventana de inscripción inválido.");

        if (!DateTime.TryParse(request.VentanaInscripcionFin, out var ventanaFin))
            return Result<Asamblea>.Failure("Formato de fin de ventana de inscripción inválido.");

        var asamblea = new Asamblea
        {
            Fecha = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Nivel = nivel,
            Tipo = tipo,
            Estado = EstadoAsamblea.PROGRAMADA,
            HorarioInicio = fecha.ToDateTime(horaInicio, DateTimeKind.Utc),
            HorarioFin = fecha.ToDateTime(horaFin, DateTimeKind.Utc),
            VentanaInscripcionInicio = ventanaInicio.ToUniversalTime(),
            VentanaInscripcionFin = ventanaFin.ToUniversalTime(),
            Lugar = request.Lugar.Trim(),
            Descripcion = request.Observaciones?.Trim()
        };

        _repo.Add(asamblea);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Asamblea creada. Id={Id}, Fecha={Fecha}, Nivel={Nivel}, Tipo={Tipo}",
            asamblea.Id, asamblea.Fecha, asamblea.Nivel, asamblea.Tipo);

        return Result<Asamblea>.Success(asamblea);
    }
}

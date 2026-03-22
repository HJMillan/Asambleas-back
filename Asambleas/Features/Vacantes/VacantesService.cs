using Asambleas.Common;
using Asambleas.Features.Asambleas;
using Asambleas.Features.Asambleas.Entities;
using Asambleas.Features.Vacantes.Entities;

namespace Asambleas.Features.Vacantes;

/// <summary>
/// Servicio de gestión de vacantes.
/// </summary>
public class VacantesService : IVacantesService
{
    private readonly IVacanteRepository _repo;
    private readonly IAsambleaRepository _asambleaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VacantesService> _logger;

    public VacantesService(
        IVacanteRepository repo,
        IAsambleaRepository asambleaRepo,
        IUnitOfWork unitOfWork,
        ILogger<VacantesService> logger)
    {
        _repo = repo;
        _asambleaRepo = asambleaRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<VacanteResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var vacantes = await _repo.GetAllWithEstablecimientoAsync(ct);
        return vacantes.Select(v => v.ToResponse()).ToList();
    }

    /// <inheritdoc />
    public async Task<Result<VacanteResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var vacante = await _repo.GetByIdWithEstablecimientoAsync(id, ct);

        if (vacante is null)
            return Result<VacanteResponse>.NotFound("Vacante no encontrada.");

        return Result<VacanteResponse>.Success(vacante.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<Vacante>> CreateAsync(CreateVacanteRequest request, CancellationToken ct = default)
    {
        // Validar enums
        if (!Enum.TryParse<NivelAsamblea>(request.Nivel, ignoreCase: true, out var nivel))
            return Result<Vacante>.Failure($"Nivel inválido: '{request.Nivel}'.");

        if (!Enum.TryParse<TipoCargo>(request.TipoCargo, ignoreCase: true, out var tipoCargo))
            return Result<Vacante>.Failure($"Tipo de cargo inválido: '{request.TipoCargo}'.");

        if (!Enum.TryParse<Turno>(request.Turno, ignoreCase: true, out var turno))
            return Result<Vacante>.Failure($"Turno inválido: '{request.Turno}'.");

        // Verificar que la asamblea existe
        var asamblea = await _asambleaRepo.GetByIdAsync(request.AsambleaId, ct);
        if (asamblea is null)
            return Result<Vacante>.NotFound($"Asamblea {request.AsambleaId} no encontrada.");

        // Buscar o crear establecimiento por código funcional
        var establecimiento = await _repo.GetEstablecimientoByCodigoAsync(
            request.Establecimiento.CodigoFuncional, ct);

        if (establecimiento is null)
        {
            establecimiento = new Establecimiento
            {
                Nombre = request.Establecimiento.Nombre.Trim(),
                Direccion = request.Establecimiento.Direccion.Trim(),
                Localidad = request.Establecimiento.Localidad.Trim(),
                CodigoFuncional = request.Establecimiento.CodigoFuncional.Trim()
            };
            _repo.AddEstablecimiento(establecimiento);
        }

        var vacante = new Vacante
        {
            Cargo = request.Cargo.Trim(),
            Nivel = nivel,
            TipoCargo = tipoCargo,
            Estado = EstadoVacante.PUBLICADA,
            Modulos = request.ModulosHoras,
            Horas = request.ModulosHoras,
            Turno = turno,
            Observaciones = request.Observaciones?.Trim(),
            AsambleaId = request.AsambleaId,
            EstablecimientoId = establecimiento.Id,
            Establecimiento = establecimiento
        };

        _repo.Add(vacante);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Vacante creada. Id={Id}, Cargo={Cargo}, Asamblea={AsambleaId}",
            vacante.Id, vacante.Cargo, vacante.AsambleaId);

        return Result<Vacante>.Success(vacante);
    }
}

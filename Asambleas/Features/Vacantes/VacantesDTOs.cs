namespace Asambleas.Features.Vacantes;

// ── Responses ──

/// <summary>
/// DTO de respuesta de vacante (coincide con el modelo frontend).
/// </summary>
public record VacanteResponse(
    Guid Id,
    string Cargo,
    string Nivel,
    EstablecimientoResponse Establecimiento,
    string TipoCargo,
    string Estado,
    Guid AsambleaId,
    bool TitularActivo,
    string FechaPublicacion,
    string? FechaBajaProgramada,
    int ModulosHoras,
    string Turno,
    string Observaciones
);

/// <summary>
/// DTO de establecimiento educativo anidado en VacanteResponse.
/// </summary>
public record EstablecimientoResponse(
    Guid Id,
    string Nombre,
    string Direccion,
    string Localidad,
    string CodigoFuncional
);

// ── Requests ──

/// <summary>
/// Request para crear una nueva vacante asociada a una asamblea.
/// </summary>
public record CreateVacanteRequest(
    string Cargo,
    string Nivel,
    string TipoCargo,
    Guid AsambleaId,
    bool TitularActivo,
    string? FechaBajaProgramada,
    int ModulosHoras,
    string Turno,
    string? Observaciones,
    CreateEstablecimientoRequest Establecimiento
);

/// <summary>
/// Datos del establecimiento. Si ya existe (por CodigoFuncional), se reutiliza.
/// </summary>
public record CreateEstablecimientoRequest(
    string Nombre,
    string Direccion,
    string Localidad,
    string CodigoFuncional
);

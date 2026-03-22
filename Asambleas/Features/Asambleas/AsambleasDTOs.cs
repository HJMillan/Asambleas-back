using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Asambleas;

// ── Responses ──

/// <summary>
/// DTO de respuesta de asamblea (coincide con el modelo frontend).
/// </summary>
public record AsambleaResponse(
    Guid Id,
    string Fecha,
    string Nivel,
    string Tipo,
    string Estado,
    string HoraInicio,
    string HoraFin,
    VentanaInscripcionResponse VentanaInscripcion,
    int VacantesCount,
    string Lugar,
    string Observaciones
);

/// <summary>
/// Ventana de inscripción con flag de si está activa actualmente.
/// </summary>
public record VentanaInscripcionResponse(
    string Inicio,
    string Fin,
    bool Activa
);

// ── Requests ──

/// <summary>
/// Request para crear una nueva asamblea.
/// </summary>
public record CreateAsambleaRequest(
    string Fecha,
    string Nivel,
    string Tipo,
    string HoraInicio,
    string HoraFin,
    string VentanaInscripcionInicio,
    string VentanaInscripcionFin,
    string Lugar,
    string? Observaciones
);

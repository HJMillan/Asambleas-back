using Asambleas.Features.Asambleas.Entities;

namespace Asambleas.Features.Asambleas;

/// <summary>
/// Extension methods para mapear entidades de Asamblea a DTOs de respuesta.
/// </summary>
public static class AsambleaMapper
{
    /// <summary>
    /// Convierte una entidad Asamblea a su DTO de respuesta.
    /// </summary>
    /// <param name="a">Entidad asamblea.</param>
    /// <param name="vacantesCount">Cantidad de vacantes asociadas (precalculada).</param>
    public static AsambleaResponse ToResponse(this Asamblea a, int vacantesCount = 0) => new(
        Id: a.Id,
        Fecha: a.Fecha.ToString("yyyy-MM-dd"),
        Nivel: a.Nivel.ToString().ToLowerInvariant(),
        Tipo: a.Tipo.ToString().ToLowerInvariant(),
        Estado: MapEstado(a.Estado),
        HoraInicio: a.HorarioInicio.ToString("HH:mm"),
        HoraFin: a.HorarioFin.ToString("HH:mm"),
        VentanaInscripcion: new VentanaInscripcionResponse(
            Inicio: a.VentanaInscripcionInicio.ToString("o"),
            Fin: a.VentanaInscripcionFin.ToString("o"),
            Activa: DateTime.UtcNow >= a.VentanaInscripcionInicio && DateTime.UtcNow <= a.VentanaInscripcionFin
        ),
        VacantesCount: vacantesCount,
        Lugar: a.Lugar,
        Observaciones: a.Descripcion ?? string.Empty
    );

    /// <summary>
    /// Mapea el enum EstadoAsamblea al formato snake_case del frontend.
    /// </summary>
    private static string MapEstado(EstadoAsamblea estado) => estado switch
    {
        EstadoAsamblea.PROGRAMADA => "programada",
        EstadoAsamblea.EN_CURSO => "en_curso",
        EstadoAsamblea.FINALIZADA => "finalizada",
        EstadoAsamblea.CANCELADA => "cancelada",
        _ => estado.ToString().ToLowerInvariant()
    };
}

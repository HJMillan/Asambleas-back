using Asambleas.Features.Vacantes.Entities;

namespace Asambleas.Features.Vacantes;

/// <summary>
/// Extension methods para mapear entidades de Vacante a DTOs de respuesta.
/// </summary>
public static class VacanteMapper
{
    /// <summary>
    /// Convierte una entidad Vacante a su DTO de respuesta.
    /// Requiere que Establecimiento esté cargado (Include).
    /// </summary>
    public static VacanteResponse ToResponse(this Vacante v) => new(
        Id: v.Id,
        Cargo: v.Cargo,
        Nivel: v.Nivel.ToString().ToLowerInvariant(),
        Establecimiento: v.Establecimiento.ToResponse(),
        TipoCargo: v.TipoCargo.ToString().ToLowerInvariant(),
        Estado: MapEstado(v.Estado),
        AsambleaId: v.AsambleaId,
        TitularActivo: v.TipoCargo == TipoCargo.SUPLENTE,
        FechaPublicacion: v.CreatedAt.ToString("o"),
        FechaBajaProgramada: null, // TODO: agregar campo a entidad cuando sea necesario
        ModulosHoras: v.Modulos > 0 ? v.Modulos : v.Horas,
        Turno: MapTurno(v.Turno),
        Observaciones: v.Observaciones ?? string.Empty
    );

    /// <summary>
    /// Convierte una entidad Establecimiento a su DTO de respuesta.
    /// </summary>
    public static EstablecimientoResponse ToResponse(this Establecimiento e) => new(
        Id: e.Id,
        Nombre: e.Nombre,
        Direccion: e.Direccion,
        Localidad: e.Localidad,
        CodigoFuncional: e.CodigoFuncional
    );

    private static string MapEstado(EstadoVacante estado) => estado switch
    {
        EstadoVacante.PUBLICADA => "publicada",
        EstadoVacante.EN_INSCRIPCION => "en_inscripcion",
        EstadoVacante.EN_EVALUACION => "en_evaluacion",
        EstadoVacante.EN_ADJUDICACION => "en_adjudicacion",
        EstadoVacante.ADJUDICADA => "adjudicada",
        EstadoVacante.DESIERTA => "desierta",
        EstadoVacante.CERRADA => "cerrada",
        EstadoVacante.CANCELADA => "cancelada",
        _ => estado.ToString().ToLowerInvariant()
    };

    private static string MapTurno(Turno turno) => turno switch
    {
        Turno.MANANA => "mañana",
        Turno.TARDE => "tarde",
        Turno.NOCHE => "noche",
        Turno.VESPERTINO => "vespertino",
        _ => turno.ToString().ToLowerInvariant()
    };
}

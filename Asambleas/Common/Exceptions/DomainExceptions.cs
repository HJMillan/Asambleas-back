namespace Asambleas.Common.Exceptions;

/// <summary>
/// Clase base abstracta para todas las excepciones de dominio.
/// Permite catch exhaustivo en GlobalExceptionHandler.
/// </summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>
/// Se lanza cuando se intenta crear una entidad que ya existe (ej: CUIL duplicado).
/// Mapeado a HTTP 409 Conflict.
/// </summary>
public class DuplicateEntityException(string message) : DomainException(message);

/// <summary>
/// Se lanza cuando una regla de negocio no se cumple.
/// Mapeado a HTTP 422 Unprocessable Entity.
/// </summary>
public class BusinessRuleException(string message) : DomainException(message);

/// <summary>
/// Se lanza cuando una entidad solicitada no se encuentra.
/// Mapeado a HTTP 404 Not Found.
/// </summary>
public class EntityNotFoundException(string entity, Guid id)
    : DomainException($"{entity} con ID {id} no fue encontrado.");

/// <summary>
/// Se lanza cuando el usuario no tiene permisos para la operación.
/// Mapeado a HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException(string message) : DomainException(message);

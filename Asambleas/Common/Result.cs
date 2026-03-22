namespace Asambleas.Common;

/// <summary>
/// Result Pattern: encapsula éxito o error sin usar excepciones para flujo de negocio.
/// StatusCode permite al controller mapear errores a HTTP status codes.
/// </summary>
public sealed class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public int? StatusCode { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;

    private Result(T value) => Value = value;
    private Result(string error, int statusCode = 400) { Error = error; StatusCode = statusCode; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, int statusCode = 400) => new(error, statusCode);
    public static Result<T> NotFound(string error) => new(error, 404);
    public static Result<T> Conflict(string error) => new(error, 409);

    /// <summary>
    /// Ejecuta onSuccess si el resultado es exitoso, o onFailure si falló.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

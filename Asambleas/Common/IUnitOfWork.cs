namespace Asambleas.Common;

/// <summary>
/// Abstracción de Unit of Work para transacciones y SaveChanges coordinado.
/// Permite que los servicios no dependan directamente de ApplicationDbContext.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Inicia una transacción de base de datos.
    /// </summary>
    Task<IDisposable> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Confirma la transacción actual.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Revierte la transacción actual.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Guarda todos los cambios pendientes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}

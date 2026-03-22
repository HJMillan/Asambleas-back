namespace Asambleas.Common.Entities;

/// <summary>
/// Entidad base con ID, timestamps de auditoría, autoría y soft-delete.
/// CreatedAt/UpdatedAt/CreatedBy/UpdatedBy se setean automáticamente
/// en ApplicationDbContext.ApplyConventions(),
/// NO en el constructor, para reflejar el momento real de persistencia.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? CreatedFromIp { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

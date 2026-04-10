namespace Domain.Entities;

public class AuditLog : Entity
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? PerformedBy { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditLog() { }

    public AuditLog(
        string entityType,
        Guid entityId,
        string action,
        string? performedBy = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        PerformedBy = performedBy;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public static AuditLog Created(string entityType, Guid entityId, string? performedBy = null, string? newValues = null)
    {
        return new AuditLog(entityType, entityId, "Created", performedBy, newValues: newValues);
    }

    public static AuditLog Updated(string entityType, Guid entityId, string? performedBy = null, string? oldValues = null, string? newValues = null)
    {
        return new AuditLog(entityType, entityId, "Updated", performedBy, oldValues, newValues);
    }

    public static AuditLog Deleted(string entityType, Guid entityId, string? performedBy = null)
    {
        return new AuditLog(entityType, entityId, "Deleted", performedBy);
    }
}

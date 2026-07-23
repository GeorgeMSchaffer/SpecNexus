namespace SargentNexus.Domain;

public abstract class EntityBase
{
    public Guid Id { get; set; }
}

public abstract class AuditableEntityBase : EntityBase
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
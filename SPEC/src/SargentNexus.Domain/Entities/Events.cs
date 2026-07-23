namespace SargentNexus.Domain;

public sealed class AuditEvent : EntityBase
{
    public Guid? OrganizationId { get; set; }

    public Guid ActorUserId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string Metadata { get; set; } = string.Empty;
}

public sealed class NotificationEvent : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Guid BoardId { get; set; }

    public Guid IdeaId { get; set; }

    public Guid ActorUserId { get; set; }

    public Guid RecipientUserId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string IdeaLink { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public string Metadata { get; set; } = string.Empty;
}
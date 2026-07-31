using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.Infrastructure.Workflow;

public sealed class NotificationWriter : INotificationWriter
{
    private readonly SargentNexusDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public NotificationWriter(SargentNexusDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task WriteAsync(
        Guid recipientUserId,
        Guid actorUserId,
        NotificationEventType eventType,
        Guid ideaId,
        string ideaTitle,
        Guid organizationId,
        Guid boardId,
        CancellationToken cancellationToken)
    {
        var message = eventType switch
        {
            NotificationEventType.IdeaMention => $"You were mentioned in idea '{ideaTitle}'.",
            NotificationEventType.CommentMention => $"You were mentioned in a comment on idea '{ideaTitle}'.",
            NotificationEventType.CommentAdded => $"A new comment was added to idea '{ideaTitle}'.",
            NotificationEventType.IdeaStatusChanged => $"The status of idea '{ideaTitle}' changed.",
            _ => $"A notification event occurred on idea '{ideaTitle}'."
        };

        var notificationEvent = new NotificationEvent
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BoardId = boardId,
            IdeaId = ideaId,
            ActorUserId = actorUserId,
            RecipientUserId = recipientUserId,
            EventType = eventType.ToString(),
            IdeaLink = $"/ideas/{ideaId}/edit",
            Message = message,
            OccurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Metadata = "{}"
        };

        _dbContext.NotificationEvents.Add(notificationEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

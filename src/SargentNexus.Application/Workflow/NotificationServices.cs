namespace SargentNexus.Application.Workflow;

public enum NotificationEventType
{
    IdeaMention,
    CommentMention,
    CommentAdded,
    IdeaStatusChanged
}

public interface INotificationWriter
{
    Task WriteAsync(
        Guid recipientUserId,
        Guid actorUserId,
        NotificationEventType eventType,
        Guid ideaId,
        string ideaTitle,
        Guid organizationId,
        Guid boardId,
        CancellationToken cancellationToken);
}

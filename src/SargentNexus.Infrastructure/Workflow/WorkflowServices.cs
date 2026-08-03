using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.Infrastructure.Workflow;

public sealed class WorkflowDataAccess : IWorkflowDataAccess
{
    private readonly SargentNexusDbContext _dbContext;

    public WorkflowDataAccess(SargentNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
    }

    public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return _dbContext.Organizations.SingleOrDefaultAsync(item => item.Id == organizationId, cancellationToken);
    }

    public Task<Status?> FindStatusByIdAsync(Guid statusId, CancellationToken cancellationToken)
    {
        return _dbContext.Statuses.SingleOrDefaultAsync(item => item.Id == statusId, cancellationToken);
    }

    public Task<Status?> FindStatusByNameAsync(Guid organizationId, string name, CancellationToken cancellationToken)
    {
        return _dbContext.Statuses.SingleOrDefaultAsync(
            item => item.OrganizationId == organizationId && item.Name == name,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Status>> ListStatusesAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Statuses
            .Where(item => item.OrganizationId == organizationId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Board?> FindBoardByIdAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return _dbContext.Boards.SingleOrDefaultAsync(item => item.Id == boardId, cancellationToken);
    }

    public async Task<IReadOnlyList<Board>> ListBoardsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Boards
            .Where(item => item.OrganizationId == organizationId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BoardSwimlane>> ListBoardSwimlanesAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return await _dbContext.BoardSwimlanes
            .Include(item => item.Status)
            .Where(item => item.BoardId == boardId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Status>> FindStatusesByIdsAsync(
        Guid organizationId,
        IReadOnlyList<Guid> statusIds,
        CancellationToken cancellationToken)
    {
        var uniqueStatusIds = statusIds.Distinct().ToArray();

        return await _dbContext.Statuses
            .Where(item =>
                item.OrganizationId == organizationId &&
                !item.IsDeleted &&
                uniqueStatusIds.Contains(item.Id))
            .ToArrayAsync(cancellationToken);
    }

    public Task<Idea?> FindIdeaByIdAsync(Guid ideaId, CancellationToken cancellationToken)
    {
        return _dbContext.Ideas
            .Include(item => item.Status)
            .Include(item => item.AuthorUser)
            .Include(item => item.AssigneeUser)
            .Include(item => item.IdeaTags)
                .ThenInclude(item => item.Tag)
            .Include(item => item.Mentions)
                .ThenInclude(item => item.MentionedUser)
            .Include(item => item.Comments)
            .Include(item => item.Upvotes)
            .SingleOrDefaultAsync(item => item.Id == ideaId && !item.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Idea>> ListIdeasByBoardIdAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return await _dbContext.Ideas
            .Include(item => item.Status)
            .Include(item => item.AuthorUser)
            .Include(item => item.AssigneeUser)
            .Include(item => item.IdeaTags)
                .ThenInclude(item => item.Tag)
            .Include(item => item.Upvotes)
            .Where(item => item.BoardId == boardId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Idea> Items, int TotalCount)> ListIdeasByOrgAsync(
        Guid organizationId,
        Guid? authorUserId,
        Guid? assigneeUserId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Ideas
            .Include(item => item.Status)
            .Include(item => item.AuthorUser)
            .Include(item => item.AssigneeUser)
            .Include(item => item.Upvotes)
            .Where(item => item.Board.OrganizationId == organizationId && !item.IsDeleted);

        if (authorUserId.HasValue)
        {
            query = query.Where(item => item.AuthorUserId == authorUserId.Value);
        }
        else if (assigneeUserId.HasValue)
        {
            query = query.Where(item => item.AssigneeUserId == assigneeUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.Title.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Comment?> FindCommentByIdAsync(Guid commentId, CancellationToken cancellationToken)
    {
        return _dbContext.Comments
            .Include(item => item.Idea)
            .SingleOrDefaultAsync(item => item.Id == commentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> ListCommentsByIdeaIdAsync(Guid ideaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Comments
            .Where(item => item.IdeaId == ideaId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<User?> FindUserByEmailAsync(Guid organizationId, string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(
            item => item.OrganizationId == organizationId && item.Email == email,
            cancellationToken);
    }

    public Task<User?> FindOrganizationUserByIdAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(
            item => item.OrganizationId == organizationId && item.Id == userId,
            cancellationToken);
    }

    public Task<Upvote?> FindUpvoteAsync(Guid ideaId, Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Upvotes.SingleOrDefaultAsync(item => item.IdeaId == ideaId && item.UserId == userId, cancellationToken);
    }

    public Task<Tag?> FindTagByNormalizedNameAsync(Guid organizationId, string normalizedName, CancellationToken cancellationToken)
    {
        return _dbContext.Tags.SingleOrDefaultAsync(
            item => item.OrganizationId == organizationId && item.NormalizedName == normalizedName,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> ListTagsByPrefixAsync(
        Guid organizationId,
        string normalizedPrefix,
        int limit,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Tags
            .Where(item => item.OrganizationId == organizationId && item.NormalizedName.StartsWith(normalizedPrefix))
            .OrderBy(item => item.NormalizedName)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public void AddStatus(Status status)
    {
        _dbContext.Statuses.Add(status);
    }

    public void AddBoard(Board board)
    {
        _dbContext.Boards.Add(board);
    }

    public void AddBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes)
    {
        _dbContext.BoardSwimlanes.AddRange(swimlanes);
    }

    public void RemoveBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes)
    {
        _dbContext.BoardSwimlanes.RemoveRange(swimlanes);
    }

    public void AddIdea(Idea idea)
    {
        _dbContext.Ideas.Add(idea);
    }

    public void AddComment(Comment comment)
    {
        _dbContext.Comments.Add(comment);
    }

    public void RemoveComment(Comment comment)
    {
        _dbContext.Comments.Remove(comment);
    }

    public void AddUpvote(Upvote upvote)
    {
        _dbContext.Upvotes.Add(upvote);
    }

    public void RemoveUpvote(Upvote upvote)
    {
        _dbContext.Upvotes.Remove(upvote);
    }

    public void AddTag(Tag tag)
    {
        _dbContext.Tags.Add(tag);
    }

    public void AddIdeaTag(IdeaTag ideaTag)
    {
        _dbContext.IdeaTags.Add(ideaTag);
    }

    public void RemoveIdeaTags(IEnumerable<IdeaTag> ideaTags)
    {
        _dbContext.IdeaTags.RemoveRange(ideaTags);
    }

    public void RemoveMentions(IEnumerable<Mention> mentions)
    {
        _dbContext.Mentions.RemoveRange(mentions);
    }

    public void AddMention(Mention mention)
    {
        _dbContext.Mentions.Add(mention);
    }

    public void AddNotificationEvent(NotificationEvent notificationEvent)
    {
        _dbContext.NotificationEvents.Add(notificationEvent);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class WorkflowAuditWriter : IWorkflowAuditWriter
{
    private readonly SargentNexusDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public WorkflowAuditWriter(SargentNexusDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public Task WriteStatusCreatedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            status.OrganizationId,
            "Status",
            status.Id,
            "Workflow.StatusCreated",
            new
            {
                status.Name,
                status.IsDeleted
            },
            cancellationToken);
    }

    public Task WriteStatusUpdatedAsync(Guid actorUserId, Status status, string previousName, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            status.OrganizationId,
            "Status",
            status.Id,
            "Workflow.StatusUpdated",
            new
            {
                PreviousName = previousName,
                status.Name,
                status.IsDeleted
            },
            cancellationToken);
    }

    public Task WriteStatusDeletedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            status.OrganizationId,
            "Status",
            status.Id,
            "Workflow.StatusDeleted",
            new
            {
                status.Name,
                status.IsDeleted
            },
            cancellationToken);
    }

    public Task WriteBoardCreatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            board.OrganizationId,
            "Board",
            board.Id,
            "Workflow.BoardCreated",
            new
            {
                board.Name,
                StatusIds = statusIds
            },
            cancellationToken);
    }

    public Task WriteBoardUpdatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, string previousName, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            board.OrganizationId,
            "Board",
            board.Id,
            "Workflow.BoardUpdated",
            new
            {
                PreviousName = previousName,
                board.Name,
                StatusIds = statusIds
            },
            cancellationToken);
    }

    public Task WriteBoardSwimlanesReorderedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> orderedStatusIds, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            board.OrganizationId,
            "Board",
            board.Id,
            "Workflow.BoardSwimlanesReordered",
            new
            {
                board.Name,
                OrderedStatusIds = orderedStatusIds
            },
            cancellationToken);
    }

    public Task WriteIdeaCreatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            idea.OrganizationId,
            "Idea",
            idea.Id,
            "Workflow.IdeaCreated",
            new
            {
                idea.BoardId,
                idea.StatusId,
                idea.Priority,
                idea.DueDate,
                idea.AssigneeUserId
            },
            cancellationToken);
    }

    public Task WriteIdeasImportedAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid boardId,
        int importedCount,
        int skippedCount,
        CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organizationId,
            "Board",
            boardId,
            "Workflow.IdeasImported",
            new
            {
                ImportedCount = importedCount,
                SkippedCount = skippedCount
            },
            cancellationToken);
    }

    public Task WriteIdeaUpdatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            idea.OrganizationId,
            "Idea",
            idea.Id,
            "Workflow.IdeaUpdated",
            new
            {
                idea.BoardId,
                idea.StatusId,
                idea.Priority,
                idea.DueDate,
                idea.AssigneeUserId,
                idea.UpdatedAtUtc
            },
            cancellationToken);
    }

    public Task WriteIdeaStatusMovedAsync(Guid actorUserId, Idea idea, Guid previousStatusId, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            idea.OrganizationId,
            "Idea",
            idea.Id,
            "Workflow.IdeaStatusMoved",
            new
            {
                PreviousStatusId = previousStatusId,
                idea.StatusId,
                idea.UpdatedAtUtc
            },
            cancellationToken);
    }

    public Task WriteCommentCreatedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organizationId,
            "Comment",
            comment.Id,
            "Workflow.CommentCreated",
            new
            {
                comment.IdeaId,
                comment.AuthorUserId,
                comment.CreatedAtUtc
            },
            cancellationToken);
    }

    public Task WriteCommentUpdatedAsync(Guid actorUserId, Guid organizationId, Comment comment, string previousBody, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organizationId,
            "Comment",
            comment.Id,
            "Workflow.CommentUpdated",
            new
            {
                comment.IdeaId,
                comment.AuthorUserId,
                PreviousBody = previousBody,
                comment.UpdatedAtUtc
            },
            cancellationToken);
    }

    public Task WriteCommentDeletedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organizationId,
            "Comment",
            comment.Id,
            "Workflow.CommentDeleted",
            new
            {
                comment.IdeaId,
                comment.AuthorUserId
            },
            cancellationToken);
    }

    public Task WriteIdeaUpvoteToggledAsync(Guid actorUserId, Idea idea, bool hasUpvoted, int upvoteCount, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            idea.OrganizationId,
            "Idea",
            idea.Id,
            "Workflow.IdeaUpvoteToggled",
            new
            {
                HasUpvoted = hasUpvoted,
                UpvoteCount = upvoteCount
            },
            cancellationToken);
    }

    private async Task WriteAsync(
        Guid actorUserId,
        Guid organizationId,
        string entityType,
        Guid entityId,
        string eventType,
        object metadata,
        CancellationToken cancellationToken)
    {
        _dbContext.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            OccurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Metadata = JsonSerializer.Serialize(metadata)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

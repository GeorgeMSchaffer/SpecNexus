using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

internal sealed class FakeWorkflowDataAccess : IWorkflowDataAccess
{
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<Guid, Organization> _organizations = new();
    private readonly Dictionary<Guid, Status> _statuses = new();
    private readonly Dictionary<Guid, Board> _boards = new();
    private readonly List<BoardSwimlane> _boardSwimlanes = new();
    private readonly Dictionary<Guid, Idea> _ideas = new();
    private readonly Dictionary<Guid, Comment> _comments = new();
    private readonly List<Upvote> _upvotes = new();
    private readonly Dictionary<Guid, Tag> _tags = new();
    private readonly List<IdeaTag> _ideaTags = new();
    private readonly Dictionary<Guid, Mention> _mentions = new();

    public List<NotificationEvent> NotificationEvents { get; } = new();

    public int SaveChangesCallCount { get; private set; }

    public void SeedUser(User user)
    {
        _users[user.Id] = user;
    }

    public void SeedOrganization(Organization organization)
    {
        _organizations[organization.Id] = organization;
    }

    public void SeedStatus(Status status)
    {
        _statuses[status.Id] = status;
    }

    public void SeedBoard(Board board)
    {
        _boards[board.Id] = board;
    }

    public void SeedSwimlane(BoardSwimlane swimlane)
    {
        AttachSwimlaneStatus(swimlane);
        _boardSwimlanes.Add(swimlane);
    }

    public void SeedIdea(Idea idea)
    {
        AttachIdeaRelations(idea);
        _ideas[idea.Id] = idea;
    }

    public void SeedComment(Comment comment)
    {
        _comments[comment.Id] = comment;

        if (_ideas.TryGetValue(comment.IdeaId, out var idea))
        {
            comment.Idea = idea;
            idea.Comments.Add(comment);
        }
    }

    public void SeedUpvote(Upvote upvote)
    {
        _upvotes.Add(upvote);

        if (_ideas.TryGetValue(upvote.IdeaId, out var idea))
        {
            idea.Upvotes.Add(upvote);
        }
    }

    public void SeedTag(Tag tag)
    {
        _tags[tag.Id] = tag;
    }

    public int TagCount => _tags.Count;

    public int IdeaTagCount => _ideaTags.Count;

    public int MentionCount => _mentions.Count;

    public Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        _organizations.TryGetValue(organizationId, out var organization);
        return Task.FromResult(organization);
    }

    public Task<Status?> FindStatusByIdAsync(Guid statusId, CancellationToken cancellationToken)
    {
        _statuses.TryGetValue(statusId, out var status);
        return Task.FromResult(status);
    }

    public Task<Status?> FindStatusByNameAsync(Guid organizationId, string name, CancellationToken cancellationToken)
    {
        var status = _statuses.Values.SingleOrDefault(item => item.OrganizationId == organizationId && item.Name == name);
        return Task.FromResult(status);
    }

    public Task<IReadOnlyList<Status>> ListStatusesAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Status>>(_statuses.Values.Where(item => item.OrganizationId == organizationId).ToArray());
    }

    public Task<Board?> FindBoardByIdAsync(Guid boardId, CancellationToken cancellationToken)
    {
        _boards.TryGetValue(boardId, out var board);
        return Task.FromResult(board);
    }

    public Task<IReadOnlyList<Board>> ListBoardsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Board>>(_boards.Values.Where(item => item.OrganizationId == organizationId).ToArray());
    }

    public Task<IReadOnlyList<BoardSwimlane>> ListBoardSwimlanesAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var swimlanes = _boardSwimlanes.Where(item => item.BoardId == boardId).ToArray();

        foreach (var swimlane in swimlanes)
        {
            AttachSwimlaneStatus(swimlane);
        }

        return Task.FromResult<IReadOnlyList<BoardSwimlane>>(swimlanes);
    }

    public Task<IReadOnlyList<Status>> FindStatusesByIdsAsync(
        Guid organizationId,
        IReadOnlyList<Guid> statusIds,
        CancellationToken cancellationToken)
    {
        var uniqueIds = statusIds.Distinct().ToHashSet();

        var statuses = _statuses.Values
            .Where(item => item.OrganizationId == organizationId && !item.IsDeleted && uniqueIds.Contains(item.Id))
            .ToArray();

        return Task.FromResult<IReadOnlyList<Status>>(statuses);
    }

    public Task<Idea?> FindIdeaByIdAsync(Guid ideaId, CancellationToken cancellationToken)
    {
        _ideas.TryGetValue(ideaId, out var idea);
        return Task.FromResult(idea);
    }

    public Task<IReadOnlyList<Idea>> ListIdeasByBoardIdAsync(Guid boardId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Idea>>(_ideas.Values.Where(item => item.BoardId == boardId).ToArray());
    }

    public Task<Comment?> FindCommentByIdAsync(Guid commentId, CancellationToken cancellationToken)
    {
        _comments.TryGetValue(commentId, out var comment);
        return Task.FromResult(comment);
    }

    public Task<IReadOnlyList<Comment>> ListCommentsByIdeaIdAsync(Guid ideaId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Comment>>(_comments.Values.Where(item => item.IdeaId == ideaId).ToArray());
    }

    public Task<User?> FindUserByEmailAsync(Guid organizationId, string email, CancellationToken cancellationToken)
    {
        var user = _users.Values.SingleOrDefault(item => item.OrganizationId == organizationId && item.Email == email);
        return Task.FromResult(user);
    }

    public Task<User?> FindOrganizationUserByIdAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        var user = _users.Values.SingleOrDefault(item => item.OrganizationId == organizationId && item.Id == userId);
        return Task.FromResult(user);
    }

    public Task<Upvote?> FindUpvoteAsync(Guid ideaId, Guid userId, CancellationToken cancellationToken)
    {
        var upvote = _upvotes.SingleOrDefault(item => item.IdeaId == ideaId && item.UserId == userId);
        return Task.FromResult(upvote);
    }

    public Task<Tag?> FindTagByNormalizedNameAsync(Guid organizationId, string normalizedName, CancellationToken cancellationToken)
    {
        var tag = _tags.Values.SingleOrDefault(item => item.OrganizationId == organizationId && item.NormalizedName == normalizedName);
        return Task.FromResult(tag);
    }

    public Task<IReadOnlyList<Tag>> ListTagsByPrefixAsync(Guid organizationId, string normalizedPrefix, int limit, CancellationToken cancellationToken)
    {
        var tags = _tags.Values
            .Where(item => item.OrganizationId == organizationId && item.NormalizedName.StartsWith(normalizedPrefix, StringComparison.Ordinal))
            .OrderBy(item => item.NormalizedName)
            .Take(limit)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Tag>>(tags);
    }

    public void AddStatus(Status status)
    {
        _statuses[status.Id] = status;
    }

    public void AddBoard(Board board)
    {
        _boards[board.Id] = board;
    }

    public void AddBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes)
    {
        foreach (var swimlane in swimlanes)
        {
            AttachSwimlaneStatus(swimlane);
            _boardSwimlanes.Add(swimlane);
        }
    }

    public void RemoveBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes)
    {
        foreach (var swimlane in swimlanes.ToArray())
        {
            _boardSwimlanes.Remove(swimlane);
        }
    }

    public void AddIdea(Idea idea)
    {
        SeedIdea(idea);
    }

    public void AddComment(Comment comment)
    {
        SeedComment(comment);
    }

    public void RemoveComment(Comment comment)
    {
        _comments.Remove(comment.Id);

        if (_ideas.TryGetValue(comment.IdeaId, out var idea))
        {
            var existing = idea.Comments.SingleOrDefault(item => item.Id == comment.Id);

            if (existing is not null)
            {
                idea.Comments.Remove(existing);
            }
        }
    }

    public void AddUpvote(Upvote upvote)
    {
        SeedUpvote(upvote);
    }

    public void RemoveUpvote(Upvote upvote)
    {
        _upvotes.RemoveAll(item => item.IdeaId == upvote.IdeaId && item.UserId == upvote.UserId);

        if (_ideas.TryGetValue(upvote.IdeaId, out var idea))
        {
            var existing = idea.Upvotes.SingleOrDefault(item => item.IdeaId == upvote.IdeaId && item.UserId == upvote.UserId);

            if (existing is not null)
            {
                idea.Upvotes.Remove(existing);
            }
        }
    }

    public void AddTag(Tag tag)
    {
        SeedTag(tag);
    }

    public void AddIdeaTag(IdeaTag ideaTag)
    {
        _ideaTags.Add(ideaTag);

        if (_ideas.TryGetValue(ideaTag.IdeaId, out var idea) && _tags.TryGetValue(ideaTag.TagId, out var tag))
        {
            idea.IdeaTags.Add(new IdeaTag
            {
                IdeaId = ideaTag.IdeaId,
                TagId = ideaTag.TagId,
                Tag = tag,
                Idea = idea
            });
        }
    }

    public void RemoveIdeaTags(IEnumerable<IdeaTag> ideaTags)
    {
        foreach (var ideaTag in ideaTags.ToArray())
        {
            _ideaTags.RemoveAll(item => item.IdeaId == ideaTag.IdeaId && item.TagId == ideaTag.TagId);

            if (_ideas.TryGetValue(ideaTag.IdeaId, out var idea))
            {
                var existing = idea.IdeaTags.SingleOrDefault(item => item.TagId == ideaTag.TagId);

                if (existing is not null)
                {
                    idea.IdeaTags.Remove(existing);
                }
            }
        }
    }

    public void RemoveMentions(IEnumerable<Mention> mentions)
    {
        foreach (var mention in mentions.ToArray())
        {
            _mentions.Remove(mention.Id);

            if (mention.IdeaId.HasValue && _ideas.TryGetValue(mention.IdeaId.Value, out var idea))
            {
                var existing = idea.Mentions.SingleOrDefault(item => item.Id == mention.Id);

                if (existing is not null)
                {
                    idea.Mentions.Remove(existing);
                }
            }
        }
    }

    public void AddMention(Mention mention)
    {
        _mentions[mention.Id] = mention;

        if (mention.IdeaId.HasValue && _ideas.TryGetValue(mention.IdeaId.Value, out var idea))
        {
            if (_users.TryGetValue(mention.MentionedUserId, out var user))
            {
                mention.MentionedUser = user;
            }

            idea.Mentions.Add(mention);
        }
    }

    public void AddNotificationEvent(NotificationEvent notificationEvent)
    {
        NotificationEvents.Add(notificationEvent);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }

    private void AttachSwimlaneStatus(BoardSwimlane swimlane)
    {
        if (_statuses.TryGetValue(swimlane.StatusId, out var status))
        {
            swimlane.Status = status;
        }
    }

    private void AttachIdeaRelations(Idea idea)
    {
        if (_boards.TryGetValue(idea.BoardId, out var board))
        {
            idea.Board = board;
        }

        if (_statuses.TryGetValue(idea.StatusId, out var status))
        {
            idea.Status = status;
        }

        if (idea.AssigneeUserId.HasValue && _users.TryGetValue(idea.AssigneeUserId.Value, out var assignee))
        {
            idea.AssigneeUser = assignee;
        }
    }
}

internal sealed class FakeWorkflowAuditWriter : IWorkflowAuditWriter
{
    public List<Status> StatusCreatedEvents { get; } = new();

    public List<(Status Status, string PreviousName)> StatusUpdatedEvents { get; } = new();

    public List<Status> StatusDeletedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> StatusIds)> BoardCreatedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> StatusIds, string PreviousName)> BoardUpdatedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> OrderedStatusIds)> BoardReorderedEvents { get; } = new();

    public List<Idea> IdeaCreatedEvents { get; } = new();

    public List<Idea> IdeaUpdatedEvents { get; } = new();

    public List<(Idea Idea, Guid PreviousStatusId)> IdeaStatusMovedEvents { get; } = new();

    public List<(Guid OrganizationId, Comment Comment)> CommentCreatedEvents { get; } = new();

    public List<(Guid OrganizationId, Comment Comment, string PreviousBody)> CommentUpdatedEvents { get; } = new();

    public List<(Guid OrganizationId, Comment Comment)> CommentDeletedEvents { get; } = new();

    public List<(Idea Idea, bool HasUpvoted, int UpvoteCount)> IdeaUpvoteToggledEvents { get; } = new();

    public Task WriteStatusCreatedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken)
    {
        StatusCreatedEvents.Add(status);
        return Task.CompletedTask;
    }

    public Task WriteStatusUpdatedAsync(Guid actorUserId, Status status, string previousName, CancellationToken cancellationToken)
    {
        StatusUpdatedEvents.Add((status, previousName));
        return Task.CompletedTask;
    }

    public Task WriteStatusDeletedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken)
    {
        StatusDeletedEvents.Add(status);
        return Task.CompletedTask;
    }

    public Task WriteBoardCreatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, CancellationToken cancellationToken)
    {
        BoardCreatedEvents.Add((board, statusIds));
        return Task.CompletedTask;
    }

    public Task WriteBoardUpdatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, string previousName, CancellationToken cancellationToken)
    {
        BoardUpdatedEvents.Add((board, statusIds, previousName));
        return Task.CompletedTask;
    }

    public Task WriteBoardSwimlanesReorderedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> orderedStatusIds, CancellationToken cancellationToken)
    {
        BoardReorderedEvents.Add((board, orderedStatusIds));
        return Task.CompletedTask;
    }

    public Task WriteIdeaCreatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken)
    {
        IdeaCreatedEvents.Add(idea);
        return Task.CompletedTask;
    }

    public Task WriteIdeaUpdatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken)
    {
        IdeaUpdatedEvents.Add(idea);
        return Task.CompletedTask;
    }

    public Task WriteIdeaStatusMovedAsync(Guid actorUserId, Idea idea, Guid previousStatusId, CancellationToken cancellationToken)
    {
        IdeaStatusMovedEvents.Add((idea, previousStatusId));
        return Task.CompletedTask;
    }

    public Task WriteCommentCreatedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken)
    {
        CommentCreatedEvents.Add((organizationId, comment));
        return Task.CompletedTask;
    }

    public Task WriteCommentUpdatedAsync(Guid actorUserId, Guid organizationId, Comment comment, string previousBody, CancellationToken cancellationToken)
    {
        CommentUpdatedEvents.Add((organizationId, comment, previousBody));
        return Task.CompletedTask;
    }

    public Task WriteCommentDeletedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken)
    {
        CommentDeletedEvents.Add((organizationId, comment));
        return Task.CompletedTask;
    }

    public Task WriteIdeaUpvoteToggledAsync(Guid actorUserId, Idea idea, bool hasUpvoted, int upvoteCount, CancellationToken cancellationToken)
    {
        IdeaUpvoteToggledEvents.Add((idea, hasUpvoted, upvoteCount));
        return Task.CompletedTask;
    }
}

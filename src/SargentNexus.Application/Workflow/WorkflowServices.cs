using System.Text.RegularExpressions;
using SargentNexus.Domain;

namespace SargentNexus.Application.Workflow;

internal sealed class MentionResolutionResult
{
    public MentionResolutionResult(IReadOnlyList<string> resolvedEmails, IReadOnlyList<string> errors)
    {
        ResolvedEmails = resolvedEmails;
        Errors = errors;
    }

    public IReadOnlyList<string> ResolvedEmails { get; }

    public IReadOnlyList<string> Errors { get; }
}

public interface IWorkflowManagementService
{
    Task<WorkflowResult<IReadOnlyList<StatusSummaryModel>>> ListStatusesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<StatusSummaryModel>> CreateStatusAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CreateStatusRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<StatusSummaryModel>> UpdateStatusAsync(
        WorkflowActorContext actor,
        Guid statusId,
        UpdateStatusRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> SoftDeleteStatusAsync(
        WorkflowActorContext actor,
        Guid statusId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IReadOnlyList<BoardSummaryModel>>> ListBoardsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<BoardDetailModel>> GetBoardDetailAsync(
        WorkflowActorContext actor,
        Guid boardId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<BoardDetailModel>> CreateBoardAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CreateBoardRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<BoardDetailModel>> UpdateBoardAsync(
        WorkflowActorContext actor,
        Guid boardId,
        UpdateBoardRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> ReorderSwimlanesAsync(
        WorkflowActorContext actor,
        Guid boardId,
        ReorderSwimlanesRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>> ListIdeasAsync(
        WorkflowActorContext actor,
        Guid boardId,
        IdeaListQueryModel query,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IdeaDetailModel>> GetIdeaDetailAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IdeaDetailModel>> CreateIdeaAsync(
        WorkflowActorContext actor,
        Guid boardId,
        IdeaWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IdeaDetailModel>> UpdateIdeaAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        IdeaWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> MoveIdeaStatusAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        MoveIdeaStatusRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<PagedResultModel<CommentModel>>> ListCommentsAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CommentListQueryModel query,
        CancellationToken cancellationToken);

    Task<WorkflowResult<CommentModel>> CreateCommentAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CommentWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<CommentModel>> UpdateCommentAsync(
        WorkflowActorContext actor,
        Guid commentId,
        CommentWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> DeleteCommentAsync(
        WorkflowActorContext actor,
        Guid commentId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<UpvoteToggleResultModel>> ToggleIdeaUpvoteAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IReadOnlyList<string>>> ListTagSuggestionsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        TagAutocompleteQueryModel query,
        CancellationToken cancellationToken);
}

public interface IWorkflowDataAccess
{
    Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Status?> FindStatusByIdAsync(Guid statusId, CancellationToken cancellationToken);

    Task<Status?> FindStatusByNameAsync(Guid organizationId, string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Status>> ListStatusesAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Board?> FindBoardByIdAsync(Guid boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Board>> ListBoardsAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BoardSwimlane>> ListBoardSwimlanesAsync(Guid boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Status>> FindStatusesByIdsAsync(Guid organizationId, IReadOnlyList<Guid> statusIds, CancellationToken cancellationToken);

    Task<Idea?> FindIdeaByIdAsync(Guid ideaId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Idea>> ListIdeasByBoardIdAsync(Guid boardId, CancellationToken cancellationToken);

    Task<Comment?> FindCommentByIdAsync(Guid commentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Comment>> ListCommentsByIdeaIdAsync(Guid ideaId, CancellationToken cancellationToken);

    Task<User?> FindUserByEmailAsync(Guid organizationId, string email, CancellationToken cancellationToken);

    Task<User?> FindOrganizationUserByIdAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);

    Task<Upvote?> FindUpvoteAsync(Guid ideaId, Guid userId, CancellationToken cancellationToken);

    Task<Tag?> FindTagByNormalizedNameAsync(Guid organizationId, string normalizedName, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tag>> ListTagsByPrefixAsync(Guid organizationId, string normalizedPrefix, int limit, CancellationToken cancellationToken);

    void AddStatus(Status status);

    void AddBoard(Board board);

    void AddBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

    void RemoveBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

    void AddIdea(Idea idea);

    void AddComment(Comment comment);

    void RemoveComment(Comment comment);

    void AddUpvote(Upvote upvote);

    void RemoveUpvote(Upvote upvote);

    void AddTag(Tag tag);

    void AddIdeaTag(IdeaTag ideaTag);

    void RemoveIdeaTags(IEnumerable<IdeaTag> ideaTags);

    void RemoveMentions(IEnumerable<Mention> mentions);

    void AddMention(Mention mention);

    void AddNotificationEvent(NotificationEvent notificationEvent);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IWorkflowAuditWriter
{
    Task WriteStatusCreatedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken);

    Task WriteStatusUpdatedAsync(Guid actorUserId, Status status, string previousName, CancellationToken cancellationToken);

    Task WriteStatusDeletedAsync(Guid actorUserId, Status status, CancellationToken cancellationToken);

    Task WriteBoardCreatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, CancellationToken cancellationToken);

    Task WriteBoardUpdatedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> statusIds, string previousName, CancellationToken cancellationToken);

    Task WriteBoardSwimlanesReorderedAsync(Guid actorUserId, Board board, IReadOnlyList<Guid> orderedStatusIds, CancellationToken cancellationToken);

    Task WriteIdeaCreatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken);

    Task WriteIdeaUpdatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken);

    Task WriteIdeaStatusMovedAsync(Guid actorUserId, Idea idea, Guid previousStatusId, CancellationToken cancellationToken);

    Task WriteCommentCreatedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken);

    Task WriteCommentUpdatedAsync(Guid actorUserId, Guid organizationId, Comment comment, string previousBody, CancellationToken cancellationToken);

    Task WriteCommentDeletedAsync(Guid actorUserId, Guid organizationId, Comment comment, CancellationToken cancellationToken);

    Task WriteIdeaUpvoteToggledAsync(Guid actorUserId, Idea idea, bool hasUpvoted, int upvoteCount, CancellationToken cancellationToken);
}

public sealed class WorkflowManagementService : IWorkflowManagementService
{
    private static readonly Regex MentionPattern = new(
        @"(?<![\w@])@([A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,})",
        RegexOptions.Compiled);

    private static readonly HashSet<UserRole> ReadRoles = new()
    {
        UserRole.SiteAdmin,
        UserRole.OrgAdmin,
        UserRole.User,
        UserRole.ReadOnly
    };

    private static readonly HashSet<UserRole> ManageRoles = new()
    {
        UserRole.SiteAdmin,
        UserRole.OrgAdmin
    };

    private static readonly HashSet<UserRole> CollaborateRoles = new()
    {
        UserRole.SiteAdmin,
        UserRole.OrgAdmin,
        UserRole.User
    };

    private static readonly HashSet<UserRole> EngageRoles = new()
    {
        UserRole.SiteAdmin,
        UserRole.OrgAdmin,
        UserRole.User,
        UserRole.ReadOnly
    };

    private readonly IWorkflowDataAccess _dataAccess;
    private readonly IWorkflowAuditWriter _auditWriter;
    private readonly INotificationWriter _notificationWriter;

    public WorkflowManagementService(
        IWorkflowDataAccess dataAccess,
        IWorkflowAuditWriter auditWriter,
        INotificationWriter notificationWriter)
    {
        _dataAccess = dataAccess;
        _auditWriter = auditWriter;
        _notificationWriter = notificationWriter;
    }

    public async Task<WorkflowResult<IReadOnlyList<StatusSummaryModel>>> ListStatusesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IReadOnlyList<StatusSummaryModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var statuses = await _dataAccess.ListStatusesAsync(organizationId, cancellationToken);
        var results = statuses
            .OrderBy(item => item.Name)
            .Select(ToStatusSummary)
            .ToArray();

        return WorkflowResult<IReadOnlyList<StatusSummaryModel>>.Success(results);
    }

    public async Task<WorkflowResult<StatusSummaryModel>> CreateStatusAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CreateStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<StatusSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Status name is required." });
        }

        var existing = await _dataAccess.FindStatusByNameAsync(organizationId, name, cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsDeleted)
            {
                return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Status name must be unique within the organization." });
            }

            existing.IsDeleted = false;
            existing.Name = name;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            await _auditWriter.WriteStatusCreatedAsync(actor.UserId, existing, cancellationToken);

            return WorkflowResult<StatusSummaryModel>.Success(ToStatusSummary(existing));
        }

        var status = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            IsDeleted = false
        };

        _dataAccess.AddStatus(status);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteStatusCreatedAsync(actor.UserId, status, cancellationToken);

        return WorkflowResult<StatusSummaryModel>.Success(ToStatusSummary(status));
    }

    public async Task<WorkflowResult<StatusSummaryModel>> UpdateStatusAsync(
        WorkflowActorContext actor,
        Guid statusId,
        UpdateStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var status = await _dataAccess.FindStatusByIdAsync(statusId, cancellationToken);

        if (status is null)
        {
            return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.StatusNotFound);
        }

        var authorization = await AuthorizeAsync(actor, status.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<StatusSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (status.IsDeleted)
        {
            return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Deleted statuses cannot be updated." });
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Status name is required." });
        }

        var duplicate = await _dataAccess.FindStatusByNameAsync(status.OrganizationId, name, cancellationToken);

        if (duplicate is not null && duplicate.Id != status.Id && !duplicate.IsDeleted)
        {
            return WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Status name must be unique within the organization." });
        }

        var previousName = status.Name;
        status.Name = name;

        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteStatusUpdatedAsync(actor.UserId, status, previousName, cancellationToken);

        return WorkflowResult<StatusSummaryModel>.Success(ToStatusSummary(status));
    }

    public async Task<WorkflowResult> SoftDeleteStatusAsync(
        WorkflowActorContext actor,
        Guid statusId,
        CancellationToken cancellationToken)
    {
        var status = await _dataAccess.FindStatusByIdAsync(statusId, cancellationToken);

        if (status is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.StatusNotFound);
        }

        var authorization = await AuthorizeAsync(actor, status.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (status.IsDeleted)
        {
            return WorkflowResult.Success();
        }

        status.IsDeleted = true;
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteStatusDeletedAsync(actor.UserId, status, cancellationToken);

        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult<IReadOnlyList<BoardSummaryModel>>> ListBoardsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IReadOnlyList<BoardSummaryModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var boards = await _dataAccess.ListBoardsAsync(organizationId, cancellationToken);
        var boardModels = new List<BoardSummaryModel>(boards.Count);

        foreach (var board in boards.OrderBy(item => item.Name))
        {
            var swimlanes = await _dataAccess.ListBoardSwimlanesAsync(board.Id, cancellationToken);
            boardModels.Add(new BoardSummaryModel
            {
                BoardId = board.Id,
                OrganizationId = board.OrganizationId,
                Name = board.Name,
                AllowUserStatusUpdate = board.AllowUserStatusUpdate,
                Swimlanes = swimlanes
                    .OrderBy(item => item.Order)
                    .Select(ToSwimlaneModel)
                    .ToArray()
            });
        }

        return WorkflowResult<IReadOnlyList<BoardSummaryModel>>.Success(boardModels);
    }

    public async Task<WorkflowResult<BoardDetailModel>> GetBoardDetailAsync(
        WorkflowActorContext actor,
        Guid boardId,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<BoardDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var swimlanes = await _dataAccess.ListBoardSwimlanesAsync(board.Id, cancellationToken);

        return WorkflowResult<BoardDetailModel>.Success(new BoardDetailModel
        {
            BoardId = board.Id,
            OrganizationId = board.OrganizationId,
            Name = board.Name,
            AllowUserStatusUpdate = board.AllowUserStatusUpdate,
            Swimlanes = swimlanes
                .OrderBy(item => item.Order)
                .Select(ToSwimlaneModel)
                .ToArray()
        });
    }

    public async Task<WorkflowResult<BoardDetailModel>> CreateBoardAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CreateBoardRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<BoardDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var validationErrors = ValidateBoardRequest(request.Name, request.StatusIds);

        if (validationErrors.Count > 0)
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.ValidationError, validationErrors);
        }

        var selectedStatuses = await _dataAccess.FindStatusesByIdsAsync(organizationId, request.StatusIds, cancellationToken);

        if (selectedStatuses.Count != request.StatusIds.Distinct().Count())
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Board statuses must exist in the organization and cannot be deleted." });
        }

        var orderedStatusIds = request.StatusIds.Distinct().ToArray();
        var statusesById = selectedStatuses.ToDictionary(item => item.Id);

        var board = new Board
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = request.Name.Trim(),
            AllowUserStatusUpdate = request.AllowUserStatusUpdate
        };

        var swimlanes = orderedStatusIds
            .Select((statusId, index) => new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = statusId,
                Order = index
            })
            .ToArray();

        _dataAccess.AddBoard(board);
        _dataAccess.AddBoardSwimlanes(swimlanes);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteBoardCreatedAsync(actor.UserId, board, orderedStatusIds, cancellationToken);

        return WorkflowResult<BoardDetailModel>.Success(new BoardDetailModel
        {
            BoardId = board.Id,
            OrganizationId = board.OrganizationId,
            Name = board.Name,
            AllowUserStatusUpdate = board.AllowUserStatusUpdate,
            Swimlanes = orderedStatusIds
                .Select((statusId, index) => new SwimlaneModel
                {
                    StatusId = statusId,
                    StatusName = statusesById[statusId].Name,
                    IsDeletedStatus = false,
                    Order = index
                })
                .ToArray()
        });
    }

    public async Task<WorkflowResult<BoardDetailModel>> UpdateBoardAsync(
        WorkflowActorContext actor,
        Guid boardId,
        UpdateBoardRequestModel request,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<BoardDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var validationErrors = ValidateBoardRequest(request.Name, request.StatusIds);

        if (validationErrors.Count > 0)
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.ValidationError, validationErrors);
        }

        var orderedStatusIds = request.StatusIds.Distinct().ToArray();
        var selectedStatuses = await _dataAccess.FindStatusesByIdsAsync(board.OrganizationId, orderedStatusIds, cancellationToken);

        if (selectedStatuses.Count != orderedStatusIds.Length)
        {
            return WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Board statuses must exist in the organization and cannot be deleted." });
        }

        var existingSwimlanes = await _dataAccess.ListBoardSwimlanesAsync(board.Id, cancellationToken);
        _dataAccess.RemoveBoardSwimlanes(existingSwimlanes);

        var newSwimlanes = orderedStatusIds
            .Select((statusId, index) => new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = statusId,
                Order = index
            })
            .ToArray();

        var previousName = board.Name;
        board.Name = request.Name.Trim();
        board.AllowUserStatusUpdate = request.AllowUserStatusUpdate;
        _dataAccess.AddBoardSwimlanes(newSwimlanes);

        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteBoardUpdatedAsync(actor.UserId, board, orderedStatusIds, previousName, cancellationToken);

        var statusesById = selectedStatuses.ToDictionary(item => item.Id);

        return WorkflowResult<BoardDetailModel>.Success(new BoardDetailModel
        {
            BoardId = board.Id,
            OrganizationId = board.OrganizationId,
            Name = board.Name,
            AllowUserStatusUpdate = board.AllowUserStatusUpdate,
            Swimlanes = orderedStatusIds
                .Select((statusId, index) => new SwimlaneModel
                {
                    StatusId = statusId,
                    StatusName = statusesById[statusId].Name,
                    IsDeletedStatus = false,
                    Order = index
                })
                .ToArray()
        });
    }

    public async Task<WorkflowResult> ReorderSwimlanesAsync(
        WorkflowActorContext actor,
        Guid boardId,
        ReorderSwimlanesRequestModel request,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var orderedStatusIds = request.OrderedStatusIds.Distinct().ToArray();

        if (orderedStatusIds.Length < 2)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.ValidationError, new[] { "A board must have at least two swimlanes." });
        }

        var existingSwimlanes = await _dataAccess.ListBoardSwimlanesAsync(boardId, cancellationToken);
        var existingStatusIds = existingSwimlanes.Select(item => item.StatusId).OrderBy(item => item).ToArray();

        if (!existingStatusIds.SequenceEqual(orderedStatusIds.OrderBy(item => item)))
        {
            return WorkflowResult.Failure(WorkflowFailureReason.ValidationError, new[] { "Swimlane reorder must include the same board statuses." });
        }

        var orderByStatusId = orderedStatusIds
            .Select((statusId, index) => new { statusId, index })
            .ToDictionary(item => item.statusId, item => item.index);

        foreach (var swimlane in existingSwimlanes)
        {
            swimlane.Order = orderByStatusId[swimlane.StatusId];
        }

        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteBoardSwimlanesReorderedAsync(actor.UserId, board, orderedStatusIds, cancellationToken);

        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>> ListIdeasAsync(
        WorkflowActorContext actor,
        Guid boardId,
        IdeaListQueryModel query,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<PagedResultModel<IdeaListItemModel>>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<PagedResultModel<IdeaListItemModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var ideas = await _dataAccess.ListIdeasByBoardIdAsync(boardId, cancellationToken);
        var filtered = ideas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filtered = filtered.Where(item =>
                item.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (query.StatusId.HasValue)
        {
            filtered = filtered.Where(item => item.StatusId == query.StatusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var normalizedTag = query.Tag.Trim().ToUpperInvariant();
            filtered = filtered.Where(item => item.IdeaTags.Any(tag => tag.Tag.NormalizedName == normalizedTag));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) && Enum.TryParse<IdeaPriority>(query.Priority, true, out var priority))
        {
            filtered = filtered.Where(item => item.Priority == priority);
        }

        if (query.DueBefore.HasValue)
        {
            filtered = filtered.Where(item => item.DueDate.HasValue && item.DueDate.Value <= query.DueBefore.Value);
        }

        filtered = OrderIdeas(filtered, query.SortBy, query.SortDirection);

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        var totalCount = filtered.Count();
        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToIdeaListItem)
            .ToArray();

        return WorkflowResult<PagedResultModel<IdeaListItemModel>>.Success(new PagedResultModel<IdeaListItemModel>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        });
    }

    public async Task<WorkflowResult<IdeaDetailModel>> GetIdeaDetailAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(idea));
    }

    public async Task<WorkflowResult<IdeaDetailModel>> CreateIdeaAsync(
        WorkflowActorContext actor,
        Guid boardId,
        IdeaWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, CollaborateRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var validationErrors = await ValidateIdeaWriteRequestAsync(board, request, cancellationToken);

        if (validationErrors.Count > 0)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, validationErrors);
        }

        var selectedStatusId = await ResolveTargetStatusIdAsync(board.Id, request.StatusId, cancellationToken);

        if (!selectedStatusId.HasValue)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "A valid board status is required." });
        }

        var parsedPriority = Enum.Parse<IdeaPriority>(request.Priority, true);
        var nowUtc = DateTime.UtcNow;
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            OrganizationId = board.OrganizationId,
            AuthorUserId = actor.UserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = parsedPriority,
            DueDate = request.DueDate,
            AssigneeUserId = request.AssigneeUserId,
            StatusId = selectedStatusId.Value,
            CreatedAtUtc = nowUtc
        };

        _dataAccess.AddIdea(idea);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        await ReplaceIdeaTagsAndMentionsAsync(idea, actor.UserId, request.TagNames, request.MentionEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        var persisted = await _dataAccess.FindIdeaByIdAsync(idea.Id, cancellationToken);
        await _auditWriter.WriteIdeaCreatedAsync(actor.UserId, persisted!, cancellationToken);
        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(persisted!));
    }

    public async Task<WorkflowResult<IdeaDetailModel>> UpdateIdeaAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        IdeaWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var board = await _dataAccess.FindBoardByIdAsync(idea.BoardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var validationErrors = await ValidateIdeaWriteRequestAsync(board, request, cancellationToken);

        if (validationErrors.Count > 0)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, validationErrors);
        }

        var selectedStatusId = await ResolveTargetStatusIdAsync(board.Id, request.StatusId ?? idea.StatusId, cancellationToken);

        if (!selectedStatusId.HasValue)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "A valid board status is required." });
        }

        idea.Title = request.Title.Trim();
        idea.Description = request.Description.Trim();
        idea.Priority = Enum.Parse<IdeaPriority>(request.Priority, true);
        idea.DueDate = request.DueDate;
        idea.AssigneeUserId = request.AssigneeUserId;
        idea.StatusId = selectedStatusId.Value;
        idea.UpdatedAtUtc = DateTime.UtcNow;

        var mentionResolution = await ResolveMentionEmailsAsync(
            idea.OrganizationId,
            $"{request.Title}\n{request.Description}",
            request.MentionEmails,
            cancellationToken);

        if (mentionResolution.Errors.Count > 0)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, mentionResolution.Errors);
        }

        _dataAccess.RemoveIdeaTags(idea.IdeaTags);
        _dataAccess.RemoveMentions(idea.Mentions);
        await ReplaceIdeaTagsAndMentionsAsync(idea, actor.UserId, request.TagNames, mentionResolution.ResolvedEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        var persisted = await _dataAccess.FindIdeaByIdAsync(idea.Id, cancellationToken);
        await _auditWriter.WriteIdeaUpdatedAsync(actor.UserId, persisted!, cancellationToken);
        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(persisted!));
    }

    public async Task<WorkflowResult> MoveIdeaStatusAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        MoveIdeaStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var board = await _dataAccess.FindBoardByIdAsync(idea.BoardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.BoardNotFound);
        }

        if (Enum.TryParse<UserRole>(actor.Role, ignoreCase: true, out var actorRole) &&
            actorRole == UserRole.User &&
            !board.AllowUserStatusUpdate)
        {
            return WorkflowResult.Failure(
                WorkflowFailureReason.Forbidden,
                new[] { "Users cannot move idea statuses on this board." });
        }

        var statusId = await ResolveTargetStatusIdAsync(idea.BoardId, request.StatusId, cancellationToken);

        if (!statusId.HasValue)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.ValidationError, new[] { "A valid board status is required." });
        }

        var now = DateTime.UtcNow;

        if (idea.ApprovalState == IdeaApprovalState.PendingApproval &&
            idea.PendingApprovalExpiresAtUtc.HasValue &&
            idea.PendingApprovalExpiresAtUtc.Value <= now)
        {
            idea.StatusId = idea.PendingApprovalPreviousStatusId ?? idea.StatusId;
            idea.ApprovalState = IdeaApprovalState.None;
            idea.PendingApprovalTargetStatusId = null;
            idea.PendingApprovalPreviousStatusId = null;
            idea.PendingApprovalRequestedAtUtc = null;
            idea.PendingApprovalExpiresAtUtc = null;
            idea.UpdatedAtUtc = now;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            return WorkflowResult.Success();
        }

        if (request.SubmitForApproval)
        {
            idea.ApprovalState = IdeaApprovalState.PendingApproval;
            idea.PendingApprovalTargetStatusId = statusId.Value;
            idea.PendingApprovalPreviousStatusId = idea.StatusId;
            idea.PendingApprovalRequestedAtUtc = now;
            idea.PendingApprovalExpiresAtUtc = now.AddHours(24);
            idea.UpdatedAtUtc = now;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            return WorkflowResult.Success();
        }

        if (request.Reject && idea.ApprovalState == IdeaApprovalState.PendingApproval)
        {
            idea.StatusId = idea.PendingApprovalPreviousStatusId ?? idea.StatusId;
            idea.ApprovalState = IdeaApprovalState.None;
            idea.PendingApprovalTargetStatusId = null;
            idea.PendingApprovalPreviousStatusId = null;
            idea.PendingApprovalRequestedAtUtc = null;
            idea.PendingApprovalExpiresAtUtc = null;
            idea.UpdatedAtUtc = now;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            return WorkflowResult.Success();
        }

        if (request.Approve && idea.ApprovalState == IdeaApprovalState.PendingApproval)
        {
            if (idea.PendingApprovalTargetStatusId != statusId.Value)
            {
                return WorkflowResult.Failure(WorkflowFailureReason.ValidationError, new[] { "The requested status does not match the pending approval target." });
            }

            var previousStatusId = idea.StatusId;
            idea.StatusId = statusId.Value;
            idea.ApprovalState = IdeaApprovalState.None;
            idea.PendingApprovalTargetStatusId = null;
            idea.PendingApprovalPreviousStatusId = null;
            idea.PendingApprovalRequestedAtUtc = null;
            idea.PendingApprovalExpiresAtUtc = null;
            idea.UpdatedAtUtc = now;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            await _auditWriter.WriteIdeaStatusMovedAsync(actor.UserId, idea, previousStatusId, cancellationToken);
            if (actor.UserId != idea.AuthorUserId)
            {
                await _notificationWriter.WriteAsync(idea.AuthorUserId, actor.UserId, NotificationEventType.IdeaStatusChanged,
                    idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
            }

            if (idea.AssigneeUserId.HasValue && idea.AssigneeUserId.Value != actor.UserId && idea.AssigneeUserId.Value != idea.AuthorUserId)
            {
                await _notificationWriter.WriteAsync(idea.AssigneeUserId.Value, actor.UserId, NotificationEventType.IdeaStatusChanged,
                    idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
            }

            return WorkflowResult.Success();
        }

        var previousStatusIdForMove = idea.StatusId;
        idea.StatusId = statusId.Value;
        idea.UpdatedAtUtc = now;
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteIdeaStatusMovedAsync(actor.UserId, idea, previousStatusIdForMove, cancellationToken);

        if (actor.UserId != idea.AuthorUserId)
        {
            await _notificationWriter.WriteAsync(idea.AuthorUserId, actor.UserId, NotificationEventType.IdeaStatusChanged,
                idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
        }

        if (idea.AssigneeUserId.HasValue && idea.AssigneeUserId.Value != actor.UserId && idea.AssigneeUserId.Value != idea.AuthorUserId)
        {
            await _notificationWriter.WriteAsync(idea.AssigneeUserId.Value, actor.UserId, NotificationEventType.IdeaStatusChanged,
                idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
        }

        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult<PagedResultModel<CommentModel>>> ListCommentsAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CommentListQueryModel query,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult<PagedResultModel<CommentModel>>.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<PagedResultModel<CommentModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var comments = await _dataAccess.ListCommentsByIdeaIdAsync(ideaId, cancellationToken);
        var ordered = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? comments.OrderByDescending(item => item.CreatedAtUtc)
            : comments.OrderBy(item => item.CreatedAtUtc);

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        var totalCount = comments.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToCommentModel)
            .ToArray();

        return WorkflowResult<PagedResultModel<CommentModel>>.Success(new PagedResultModel<CommentModel>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        });
    }

    public async Task<WorkflowResult<CommentModel>> CreateCommentAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CommentWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<CommentModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var body = request.Body?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(body))
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Comment body is required." });
        }

        var mentionResolution = await ResolveMentionEmailsAsync(idea.OrganizationId, body, Array.Empty<string>(), cancellationToken);

        if (mentionResolution.Errors.Count > 0)
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.ValidationError, mentionResolution.Errors);
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = ideaId,
            AuthorUserId = actor.UserId,
            Body = body,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dataAccess.AddComment(comment);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await AddCommentMentionsAsync(comment, idea, actor.UserId, mentionResolution.ResolvedEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteCommentCreatedAsync(actor.UserId, idea.OrganizationId, comment, cancellationToken);

        if (actor.UserId != idea.AuthorUserId)
        {
            await _notificationWriter.WriteAsync(idea.AuthorUserId, actor.UserId, NotificationEventType.CommentAdded,
                idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
        }

        if (idea.AssigneeUserId.HasValue && idea.AssigneeUserId.Value != actor.UserId && idea.AssigneeUserId.Value != idea.AuthorUserId)
        {
            await _notificationWriter.WriteAsync(idea.AssigneeUserId.Value, actor.UserId, NotificationEventType.CommentAdded,
                idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
        }

        return WorkflowResult<CommentModel>.Success(ToCommentModel(comment));
    }

    public async Task<WorkflowResult<CommentModel>> UpdateCommentAsync(
        WorkflowActorContext actor,
        Guid commentId,
        CommentWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var comment = await _dataAccess.FindCommentByIdAsync(commentId, cancellationToken);

        if (comment is null)
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.CommentNotFound);
        }

        var authorization = await AuthorizeAsync(actor, comment.Idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<CommentModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (comment.AuthorUserId != actor.UserId)
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.Forbidden);
        }

        var body = request.Body?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(body))
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "Comment body is required." });
        }

        var mentionResolution = await ResolveMentionEmailsAsync(comment.Idea.OrganizationId, body, Array.Empty<string>(), cancellationToken);

        if (mentionResolution.Errors.Count > 0)
        {
            return WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.ValidationError, mentionResolution.Errors);
        }

        var previousBody = comment.Body;
        comment.Body = body;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        _dataAccess.RemoveMentions(comment.Mentions);
        await AddCommentMentionsAsync(comment, comment.Idea, actor.UserId, mentionResolution.ResolvedEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteCommentUpdatedAsync(actor.UserId, comment.Idea.OrganizationId, comment, previousBody, cancellationToken);

        return WorkflowResult<CommentModel>.Success(ToCommentModel(comment));
    }

    public async Task<WorkflowResult> DeleteCommentAsync(
        WorkflowActorContext actor,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var comment = await _dataAccess.FindCommentByIdAsync(commentId, cancellationToken);

        if (comment is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.CommentNotFound);
        }

        var authorization = await AuthorizeAsync(actor, comment.Idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var canDelete = comment.AuthorUserId == actor.UserId ||
            string.Equals(actor.Role, UserRole.OrgAdmin.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(actor.Role, UserRole.SiteAdmin.ToString(), StringComparison.OrdinalIgnoreCase);

        if (!canDelete)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        _dataAccess.RemoveComment(comment);
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteCommentDeletedAsync(actor.UserId, comment.Idea.OrganizationId, comment, cancellationToken);

        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult<UpvoteToggleResultModel>> ToggleIdeaUpvoteAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult<UpvoteToggleResultModel>.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, EngageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<UpvoteToggleResultModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var existingUpvote = await _dataAccess.FindUpvoteAsync(ideaId, actor.UserId, cancellationToken);
        var hasUpvoted = false;

        if (existingUpvote is null)
        {
            _dataAccess.AddUpvote(new Upvote
            {
                IdeaId = ideaId,
                UserId = actor.UserId,
                CreatedAtUtc = DateTime.UtcNow
            });
            hasUpvoted = true;
        }
        else
        {
            _dataAccess.RemoveUpvote(existingUpvote);
        }

        await _dataAccess.SaveChangesAsync(cancellationToken);
        var refreshed = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);
        await _auditWriter.WriteIdeaUpvoteToggledAsync(actor.UserId, refreshed!, hasUpvoted, refreshed!.Upvotes.Count, cancellationToken);

        return WorkflowResult<UpvoteToggleResultModel>.Success(new UpvoteToggleResultModel
        {
            IdeaId = ideaId,
            HasUpvoted = hasUpvoted,
            UpvoteCount = refreshed!.Upvotes.Count
        });
    }

    public async Task<WorkflowResult<IReadOnlyList<string>>> ListTagSuggestionsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        TagAutocompleteQueryModel query,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, CollaborateRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IReadOnlyList<string>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var search = query.Search?.Trim() ?? string.Empty;

        if (search.Length < 2)
        {
            return WorkflowResult<IReadOnlyList<string>>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Tag autocomplete requires at least 2 characters." });
        }

        var limit = query.Limit <= 0 ? 10 : Math.Min(query.Limit, 50);
        var normalizedPrefix = search.ToUpperInvariant();
        var tags = await _dataAccess.ListTagsByPrefixAsync(organizationId, normalizedPrefix, limit, cancellationToken);

        var suggestions = tags
            .Select(item => item.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return WorkflowResult<IReadOnlyList<string>>.Success(suggestions);
    }

    private static SwimlaneModel ToSwimlaneModel(BoardSwimlane swimlane)
    {
        return new SwimlaneModel
        {
            StatusId = swimlane.StatusId,
            StatusName = swimlane.Status.IsDeleted
                ? $"{swimlane.Status.Name} (Deleted)"
                : swimlane.Status.Name,
            IsDeletedStatus = swimlane.Status.IsDeleted,
            Order = swimlane.Order
        };
    }

    private static IEnumerable<Idea> OrderIdeas(IEnumerable<Idea> ideas, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "updatedat" => descending ? ideas.OrderByDescending(item => item.UpdatedAtUtc ?? item.CreatedAtUtc) : ideas.OrderBy(item => item.UpdatedAtUtc ?? item.CreatedAtUtc),
            "upvotecount" => descending ? ideas.OrderByDescending(item => item.Upvotes.Count) : ideas.OrderBy(item => item.Upvotes.Count),
            "priority" => descending ? ideas.OrderByDescending(item => item.Priority) : ideas.OrderBy(item => item.Priority),
            "duedate" => descending ? ideas.OrderByDescending(item => item.DueDate) : ideas.OrderBy(item => item.DueDate),
            _ => descending ? ideas.OrderByDescending(item => item.CreatedAtUtc) : ideas.OrderBy(item => item.CreatedAtUtc)
        };
    }

    private static IdeaListItemModel ToIdeaListItem(Idea idea)
    {
        return new IdeaListItemModel
        {
            IdeaId = idea.Id,
            BoardId = idea.BoardId,
            Title = idea.Title,
            Priority = idea.Priority.ToString(),
            DueDate = idea.DueDate,
            AssigneeUserId = idea.AssigneeUserId,
            AssigneeDisplayName = idea.AssigneeUser is null
                ? null
                : $"{idea.AssigneeUser.FirstName} {idea.AssigneeUser.LastName}".Trim(),
            StatusId = idea.StatusId,
            StatusName = idea.Status.Name,
            UpvoteCount = idea.Upvotes.Count,
            AuthorUserId = idea.AuthorUserId,
            CreatedAtUtc = idea.CreatedAtUtc
        };
    }

    private static IdeaDetailModel ToIdeaDetail(Idea idea)
    {
        return new IdeaDetailModel
        {
            IdeaId = idea.Id,
            BoardId = idea.BoardId,
            Title = idea.Title,
            Description = idea.Description,
            Priority = idea.Priority.ToString(),
            DueDate = idea.DueDate,
            AssigneeUserId = idea.AssigneeUserId,
            AssigneeDisplayName = idea.AssigneeUser is null
                ? null
                : $"{idea.AssigneeUser.FirstName} {idea.AssigneeUser.LastName}".Trim(),
            StatusId = idea.StatusId,
            StatusName = idea.Status.Name,
            ApprovalState = idea.ApprovalState.ToString(),
            PendingApprovalTargetStatusId = idea.PendingApprovalTargetStatusId,
            PendingApprovalPreviousStatusId = idea.PendingApprovalPreviousStatusId,
            PendingApprovalRequestedAtUtc = idea.PendingApprovalRequestedAtUtc,
            PendingApprovalExpiresAtUtc = idea.PendingApprovalExpiresAtUtc,
            TagNames = idea.IdeaTags
                .Select(item => item.Tag.Name)
                .OrderBy(item => item)
                .ToArray(),
            Mentions = idea.Mentions
                .Select(item => item.MentionedUser.Email)
                .OrderBy(item => item)
                .ToArray(),
            Comments = idea.Comments
                .OrderBy(item => item.CreatedAtUtc)
                .Select(ToCommentModel)
                .ToArray(),
            UpvoteCount = idea.Upvotes.Count
        };
    }

    private static CommentModel ToCommentModel(Comment comment)
    {
        return new CommentModel
        {
            CommentId = comment.Id,
            IdeaId = comment.IdeaId,
            AuthorUserId = comment.AuthorUserId,
            Body = comment.Body,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc
        };
    }

    private async Task<IReadOnlyList<string>> ValidateIdeaWriteRequestAsync(
        Board board,
        IdeaWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title?.Trim()))
        {
            errors.Add("Idea title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description?.Trim()))
        {
            errors.Add("Idea description is required.");
        }

        if (!Enum.TryParse<IdeaPriority>(request.Priority, true, out _))
        {
            errors.Add("Idea priority must be one of Low, Medium, High, or Critical.");
        }

        if (request.AssigneeUserId.HasValue)
        {
            var assignee = await _dataAccess.FindOrganizationUserByIdAsync(board.OrganizationId, request.AssigneeUserId.Value, cancellationToken);

            if (assignee is null)
            {
                errors.Add("Assignee user must belong to the board organization.");
            }
        }

        var mentionResolution = await ResolveMentionEmailsAsync(
            board.OrganizationId,
            $"{request.Title}\n{request.Description}",
            request.MentionEmails,
            cancellationToken);

        errors.AddRange(mentionResolution.Errors);

        foreach (var tagName in request.TagNames)
        {
            if (string.IsNullOrWhiteSpace(tagName.Trim()))
            {
                errors.Add("Tag names cannot be empty.");
                break;
            }
        }

        return errors;
    }

    private async Task<Guid?> ResolveTargetStatusIdAsync(Guid boardId, Guid? requestedStatusId, CancellationToken cancellationToken)
    {
        var swimlanes = await _dataAccess.ListBoardSwimlanesAsync(boardId, cancellationToken);
        var orderedStatusIds = swimlanes.OrderBy(item => item.Order).Select(item => item.StatusId).ToArray();

        if (orderedStatusIds.Length == 0)
        {
            return null;
        }

        if (!requestedStatusId.HasValue)
        {
            return orderedStatusIds[0];
        }

        return orderedStatusIds.Contains(requestedStatusId.Value)
            ? requestedStatusId
            : null;
    }

    private async Task ReplaceIdeaTagsAndMentionsAsync(
        Idea idea,
        Guid actorUserId,
        IReadOnlyList<string> tagNames,
        IReadOnlyList<string> mentionEmails,
        CancellationToken cancellationToken)
    {
        var distinctTagNames = tagNames
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var tagName in distinctTagNames)
        {
            var normalizedName = tagName.ToUpperInvariant();
            var tag = await _dataAccess.FindTagByNormalizedNameAsync(idea.OrganizationId, normalizedName, cancellationToken);

            if (tag is null)
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = idea.OrganizationId,
                    Name = tagName,
                    NormalizedName = normalizedName
                };

                _dataAccess.AddTag(tag);
            }

            _dataAccess.AddIdeaTag(new IdeaTag
            {
                IdeaId = idea.Id,
                TagId = tag.Id
            });
        }

        await AddIdeaMentionsAsync(idea, actorUserId, mentionEmails, cancellationToken);
    }

    private async Task<MentionResolutionResult> ResolveMentionEmailsAsync(
        Guid organizationId,
        string? text,
        IReadOnlyList<string>? explicitEmails,
        CancellationToken cancellationToken)
    {
        var candidates = new List<string>();
        var errors = new List<string>();

        if (explicitEmails is not null)
        {
            foreach (var explicitEmail in explicitEmails)
            {
                var email = explicitEmail?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add("Mention emails cannot be empty.");
                    continue;
                }

                candidates.Add(email);
            }
        }

        foreach (Match match in MentionPattern.Matches(text ?? string.Empty))
        {
            candidates.Add(match.Groups[1].Value.Trim());
        }

        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                errors.Add("Mention emails cannot be empty.");
                continue;
            }

            var mentionedUser = await _dataAccess.FindUserByEmailAsync(organizationId, candidate, cancellationToken);

            if (mentionedUser is null)
            {
                errors.Add($"Mention email '{candidate}' must belong to the board organization.");
                continue;
            }

            resolved.Add(candidate);
        }

        return new MentionResolutionResult(
            resolved.OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToArray(),
            errors);
    }

    private async Task AddIdeaMentionsAsync(
        Idea idea,
        Guid actorUserId,
        IReadOnlyList<string> mentionEmails,
        CancellationToken cancellationToken)
    {
        foreach (var email in mentionEmails)
        {
            var mentionedUser = await _dataAccess.FindUserByEmailAsync(idea.OrganizationId, email, cancellationToken);

            if (mentionedUser is null)
            {
                continue;
            }

            _dataAccess.AddMention(new Mention
            {
                Id = Guid.NewGuid(),
                OrganizationId = idea.OrganizationId,
                IdeaId = idea.Id,
                MentionedUserId = mentionedUser.Id,
                SourceText = email
            });

            if (mentionedUser.Id != actorUserId)
            {
                await _notificationWriter.WriteAsync(mentionedUser.Id, actorUserId, NotificationEventType.IdeaMention,
                    idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
            }
        }
    }

    private async Task AddCommentMentionsAsync(
        Comment comment,
        Idea idea,
        Guid actorUserId,
        IReadOnlyList<string> mentionEmails,
        CancellationToken cancellationToken)
    {
        foreach (var email in mentionEmails)
        {
            var mentionedUser = await _dataAccess.FindUserByEmailAsync(idea.OrganizationId, email, cancellationToken);

            if (mentionedUser is null)
            {
                continue;
            }

            _dataAccess.AddMention(new Mention
            {
                Id = Guid.NewGuid(),
                OrganizationId = idea.OrganizationId,
                CommentId = comment.Id,
                MentionedUserId = mentionedUser.Id,
                SourceText = email
            });

            if (mentionedUser.Id != actorUserId)
            {
                await _notificationWriter.WriteAsync(mentionedUser.Id, actorUserId, NotificationEventType.CommentMention,
                    idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
            }
        }
    }

    private static string BuildIdeaLink(Idea idea)
    {
        return $"/ideas/{idea.Id}/edit";
    }

    private static StatusSummaryModel ToStatusSummary(Status status)
    {
        return new StatusSummaryModel
        {
            StatusId = status.Id,
            OrganizationId = status.OrganizationId,
            Name = status.Name,
            IsDeleted = status.IsDeleted
        };
    }

    private async Task<WorkflowResult> AuthorizeAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        IReadOnlySet<UserRole> allowedRoles,
        CancellationToken cancellationToken)
    {
        if (actor.UserId == Guid.Empty)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Unauthorized);
        }

        if (!Enum.TryParse<UserRole>(actor.Role, ignoreCase: true, out var actorRole))
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        if (!allowedRoles.Contains(actorRole))
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        if (actorRole != UserRole.SiteAdmin && actor.OrganizationId != organizationId)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        var user = await _dataAccess.FindUserByIdAsync(actor.UserId, cancellationToken);

        if (user is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Unauthorized);
        }

        if (user.Role != actorRole)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        if (user.Role != UserRole.SiteAdmin && user.OrganizationId != organizationId)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.Forbidden);
        }

        var organization = await _dataAccess.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.OrganizationNotFound);
        }

        return WorkflowResult.Success();
    }

    private static List<string> ValidateBoardRequest(string name, IReadOnlyList<Guid> statusIds)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name?.Trim()))
        {
            errors.Add("Board name is required.");
        }

        if (statusIds.Count < 2)
        {
            errors.Add("A board must have at least two swimlanes.");
        }

        if (statusIds.Any(item => item == Guid.Empty))
        {
            errors.Add("Swimlane status identifiers are required.");
        }

        return errors;
    }
}

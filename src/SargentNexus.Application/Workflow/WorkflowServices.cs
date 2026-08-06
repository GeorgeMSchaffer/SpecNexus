using System.Globalization;
using System.Text;
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

    Task<WorkflowResult<IReadOnlyList<IdeaTypeSummaryModel>>> ListIdeaTypesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IdeaTypeSummaryModel>> CreateIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        IdeaTypeWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IdeaTypeSummaryModel>> UpdateIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid ideaTypeId,
        IdeaTypeWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> ReorderIdeaTypesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        ReorderIdeaTypesRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> SoftDeleteIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid ideaTypeId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<IReadOnlyList<BusinessImpactSummaryModel>>> ListBusinessImpactsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<WorkflowResult<BusinessImpactSummaryModel>> CreateBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult<BusinessImpactSummaryModel>> UpdateBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid businessImpactId,
        BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> ReorderBusinessImpactsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        ReorderBusinessImpactsRequestModel request,
        CancellationToken cancellationToken);

    Task<WorkflowResult> SoftDeleteBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid businessImpactId,
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

    Task<WorkflowImportResult<IdeaImportResponseModel>> ImportIdeasCsvAsync(
        WorkflowActorContext actor,
        Guid boardId,
        byte[] fileBytes,
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

    Task<WorkflowResult> SoftDeleteIdeaAsync(
        WorkflowActorContext actor,
        Guid ideaId,
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

    Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>> ListMyIdeasAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        OrgIdeaListQueryModel query,
        CancellationToken cancellationToken);
}

public interface IWorkflowDataAccess
{
    Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Status?> FindStatusByIdAsync(Guid statusId, CancellationToken cancellationToken);

    Task<Status?> FindStatusByNameAsync(Guid organizationId, string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Status>> ListStatusesAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<IdeaType?> FindIdeaTypeByIdAsync(Guid ideaTypeId, CancellationToken cancellationToken);

    Task<BusinessImpact?> FindBusinessImpactByIdAsync(Guid businessImpactId, CancellationToken cancellationToken);

    Task<IReadOnlyList<IdeaType>> ListIdeaTypesAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BusinessImpact>> ListBusinessImpactsAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Board?> FindBoardByIdAsync(Guid boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Board>> ListBoardsAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BoardSwimlane>> ListBoardSwimlanesAsync(Guid boardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Status>> FindStatusesByIdsAsync(Guid organizationId, IReadOnlyList<Guid> statusIds, CancellationToken cancellationToken);

    Task<Idea?> FindIdeaByIdAsync(Guid ideaId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Idea>> ListIdeasByBoardIdAsync(Guid boardId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Idea> Items, int TotalCount)> ListIdeasByOrgAsync(
        Guid organizationId,
        Guid? authorUserId,
        Guid? assigneeUserId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Comment?> FindCommentByIdAsync(Guid commentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Comment>> ListCommentsByIdeaIdAsync(Guid ideaId, CancellationToken cancellationToken);

    Task<User?> FindUserByEmailAsync(Guid organizationId, string email, CancellationToken cancellationToken);

    Task<User?> FindOrganizationUserByIdAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);

    Task<Upvote?> FindUpvoteAsync(Guid ideaId, Guid userId, CancellationToken cancellationToken);

    Task<Tag?> FindTagByNormalizedNameAsync(Guid organizationId, string normalizedName, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tag>> ListTagsByPrefixAsync(Guid organizationId, string normalizedPrefix, int limit, CancellationToken cancellationToken);

    void AddStatus(Status status);

    void AddIdeaType(IdeaType ideaType);

    void AddBusinessImpact(BusinessImpact businessImpact);

    void AddBoard(Board board);

    void AddBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

    void RemoveBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

    void AddIdea(Idea idea);

    void AddIdeaAssignee(IdeaAssignee ideaAssignee);

    void RemoveIdeaAssignees(IEnumerable<IdeaAssignee> ideaAssignees);

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

    Task WriteIdeasImportedAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid boardId,
        int importedCount,
        int skippedCount,
        CancellationToken cancellationToken);

    Task WriteIdeaUpdatedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken);

    Task WriteIdeaDeletedAsync(Guid actorUserId, Idea idea, CancellationToken cancellationToken);

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

    private static readonly Regex BusinessImpactColorPattern = new(
        "^#[0-9A-Fa-f]{6}$",
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
            existing.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
            existing.SortOrder = request.SortOrder;
            existing.IsDefault = request.IsDefault;
            await _dataAccess.SaveChangesAsync(cancellationToken);
            await _auditWriter.WriteStatusCreatedAsync(actor.UserId, existing, cancellationToken);

            return WorkflowResult<StatusSummaryModel>.Success(ToStatusSummary(existing));
        }

        var status = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            IsDeleted = false,
            Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(),
            SortOrder = request.SortOrder,
            IsDefault = request.IsDefault
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
        status.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        status.SortOrder = request.SortOrder;
        status.IsDefault = request.IsDefault;

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

    public async Task<WorkflowResult<IReadOnlyList<IdeaTypeSummaryModel>>> ListIdeaTypesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IReadOnlyList<IdeaTypeSummaryModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var ideaTypes = await _dataAccess.ListIdeaTypesAsync(organizationId, cancellationToken);
        var results = ideaTypes
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(ToIdeaTypeSummary)
            .ToArray();

        return WorkflowResult<IReadOnlyList<IdeaTypeSummaryModel>>.Success(results);
    }

    public async Task<WorkflowResult<IdeaTypeSummaryModel>> CreateIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        IdeaTypeWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var name = request.Name.Trim();
        var nameError = ValidateOptionName(name, "Idea Type");

        if (nameError is not null)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { nameError });
        }

        var ideaTypes = await _dataAccess.ListIdeaTypesAsync(organizationId, cancellationToken);

        if (ideaTypes.Any(item => !item.IsDeleted && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Idea Type name must be unique within the organization." });
        }

        var ideaType = new IdeaType
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            SortOrder = ideaTypes.Count == 0 ? 0 : ideaTypes.Max(item => item.SortOrder) + 1,
            IsDeleted = false
        };

        _dataAccess.AddIdeaType(ideaType);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        return WorkflowResult<IdeaTypeSummaryModel>.Success(ToIdeaTypeSummary(ideaType));
    }

    public async Task<WorkflowResult<IdeaTypeSummaryModel>> UpdateIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid ideaTypeId,
        IdeaTypeWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var ideaType = await _dataAccess.FindIdeaTypeByIdAsync(ideaTypeId, cancellationToken);

        if (ideaType is null)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(WorkflowFailureReason.IdeaTypeNotFound);
        }

        var authorization = await AuthorizeAsync(actor, ideaType.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (ideaType.IsDeleted)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Archived Idea Types cannot be updated." });
        }

        var name = request.Name.Trim();
        var nameError = ValidateOptionName(name, "Idea Type");

        if (nameError is not null)
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { nameError });
        }

        var ideaTypes = await _dataAccess.ListIdeaTypesAsync(ideaType.OrganizationId, cancellationToken);

        if (ideaTypes.Any(item =>
            item.Id != ideaType.Id &&
            !item.IsDeleted &&
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return WorkflowResult<IdeaTypeSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Idea Type name must be unique within the organization." });
        }

        ideaType.Name = name;
        await _dataAccess.SaveChangesAsync(cancellationToken);

        return WorkflowResult<IdeaTypeSummaryModel>.Success(ToIdeaTypeSummary(ideaType));
    }

    public async Task<WorkflowResult> ReorderIdeaTypesAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        ReorderIdeaTypesRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var ideaTypes = await _dataAccess.ListIdeaTypesAsync(organizationId, cancellationToken);
        var requestedIds = request.OrderedIdeaTypeIds;

        if (!IsCompleteReorder(ideaTypes.Select(item => item.Id), requestedIds))
        {
            return WorkflowResult.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Idea Type reorder must include every organization option exactly once." });
        }

        var orderById = requestedIds
            .Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        foreach (var ideaType in ideaTypes)
        {
            ideaType.SortOrder = orderById[ideaType.Id];
        }

        await _dataAccess.SaveChangesAsync(cancellationToken);
        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult> SoftDeleteIdeaTypeAsync(
        WorkflowActorContext actor,
        Guid ideaTypeId,
        CancellationToken cancellationToken)
    {
        var ideaType = await _dataAccess.FindIdeaTypeByIdAsync(ideaTypeId, cancellationToken);

        if (ideaType is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.IdeaTypeNotFound);
        }

        var authorization = await AuthorizeAsync(actor, ideaType.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (ideaType.IsDeleted)
        {
            return WorkflowResult.Success();
        }

        var ideaTypes = await _dataAccess.ListIdeaTypesAsync(ideaType.OrganizationId, cancellationToken);

        if (ideaTypes.Count(item => !item.IsDeleted) <= 1)
        {
            return WorkflowResult.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "The last active Idea Type cannot be deleted." });
        }

        ideaType.IsDeleted = true;
        await _dataAccess.SaveChangesAsync(cancellationToken);

        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult<IReadOnlyList<BusinessImpactSummaryModel>>> ListBusinessImpactsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IReadOnlyList<BusinessImpactSummaryModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var businessImpacts = await _dataAccess.ListBusinessImpactsAsync(organizationId, cancellationToken);
        var results = businessImpacts
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(ToBusinessImpactSummary)
            .ToArray();

        return WorkflowResult<IReadOnlyList<BusinessImpactSummaryModel>>.Success(results);
    }

    public async Task<WorkflowResult<BusinessImpactSummaryModel>> CreateBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var name = request.Name.Trim();
        var nameError = ValidateOptionName(name, "Business Impact");
        var color = request.Color.Trim();

        if (nameError is not null)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { nameError });
        }

        if (!BusinessImpactColorPattern.IsMatch(color))
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Business Impact color must be a valid #RRGGBB value." });
        }

        var businessImpacts = await _dataAccess.ListBusinessImpactsAsync(organizationId, cancellationToken);

        if (businessImpacts.Any(item => !item.IsDeleted && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Business Impact name must be unique within the organization." });
        }

        var businessImpact = new BusinessImpact
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            Color = color.ToUpperInvariant(),
            SortOrder = businessImpacts.Count == 0 ? 0 : businessImpacts.Max(item => item.SortOrder) + 1,
            IsDeleted = false
        };

        _dataAccess.AddBusinessImpact(businessImpact);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        return WorkflowResult<BusinessImpactSummaryModel>.Success(ToBusinessImpactSummary(businessImpact));
    }

    public async Task<WorkflowResult<BusinessImpactSummaryModel>> UpdateBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid businessImpactId,
        BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var businessImpact = await _dataAccess.FindBusinessImpactByIdAsync(businessImpactId, cancellationToken);

        if (businessImpact is null)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(WorkflowFailureReason.BusinessImpactNotFound);
        }

        var authorization = await AuthorizeAsync(actor, businessImpact.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (businessImpact.IsDeleted)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Archived Business Impacts cannot be updated." });
        }

        var name = request.Name.Trim();
        var nameError = ValidateOptionName(name, "Business Impact");
        var color = request.Color.Trim();

        if (nameError is not null)
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(WorkflowFailureReason.ValidationError, new[] { nameError });
        }

        if (!BusinessImpactColorPattern.IsMatch(color))
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Business Impact color must be a valid #RRGGBB value." });
        }

        var businessImpacts = await _dataAccess.ListBusinessImpactsAsync(businessImpact.OrganizationId, cancellationToken);

        if (businessImpacts.Any(item =>
            item.Id != businessImpact.Id &&
            !item.IsDeleted &&
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return WorkflowResult<BusinessImpactSummaryModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Business Impact name must be unique within the organization." });
        }

        businessImpact.Name = name;
        businessImpact.Color = color.ToUpperInvariant();
        await _dataAccess.SaveChangesAsync(cancellationToken);

        return WorkflowResult<BusinessImpactSummaryModel>.Success(ToBusinessImpactSummary(businessImpact));
    }

    public async Task<WorkflowResult> ReorderBusinessImpactsAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        ReorderBusinessImpactsRequestModel request,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var businessImpacts = await _dataAccess.ListBusinessImpactsAsync(organizationId, cancellationToken);
        var requestedIds = request.OrderedBusinessImpactIds;

        if (!IsCompleteReorder(businessImpacts.Select(item => item.Id), requestedIds))
        {
            return WorkflowResult.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "Business Impact reorder must include every organization option exactly once." });
        }

        var orderById = requestedIds
            .Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        foreach (var businessImpact in businessImpacts)
        {
            businessImpact.SortOrder = orderById[businessImpact.Id];
        }

        await _dataAccess.SaveChangesAsync(cancellationToken);
        return WorkflowResult.Success();
    }

    public async Task<WorkflowResult> SoftDeleteBusinessImpactAsync(
        WorkflowActorContext actor,
        Guid businessImpactId,
        CancellationToken cancellationToken)
    {
        var businessImpact = await _dataAccess.FindBusinessImpactByIdAsync(businessImpactId, cancellationToken);

        if (businessImpact is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.BusinessImpactNotFound);
        }

        var authorization = await AuthorizeAsync(actor, businessImpact.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        if (businessImpact.IsDeleted)
        {
            return WorkflowResult.Success();
        }

        var businessImpacts = await _dataAccess.ListBusinessImpactsAsync(businessImpact.OrganizationId, cancellationToken);

        if (businessImpacts.Count(item => !item.IsDeleted) <= 1)
        {
            return WorkflowResult.Failure(
                WorkflowFailureReason.ValidationError,
                new[] { "The last active Business Impact cannot be deleted." });
        }

        businessImpact.IsDeleted = true;
        await _dataAccess.SaveChangesAsync(cancellationToken);

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
                IsArchived = board.IsArchived,
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
        var filtered = ideas.AsEnumerable().Where(item => !item.IsDeleted);

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
            .Select(item => ToIdeaListItem(item, actor.UserId))
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

        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(idea, actor.UserId));
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
            IdeaTypeId = request.IdeaTypeId,
            BusinessImpactId = request.BusinessImpactId,
            DueDate = request.DueDate,
            StatusId = selectedStatusId.Value,
            CreatedAtUtc = nowUtc
        };

        _dataAccess.AddIdea(idea);
        await ReplaceIdeaAssigneesAsync(idea, request.AssigneeUserIds, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        await ReplaceIdeaTagsAndMentionsAsync(idea, actor.UserId, request.TagNames, request.MentionEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        var persisted = await _dataAccess.FindIdeaByIdAsync(idea.Id, cancellationToken);
        await _auditWriter.WriteIdeaCreatedAsync(actor.UserId, persisted!, cancellationToken);
        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(persisted!, actor.UserId));
    }

    public async Task<WorkflowImportResult<IdeaImportResponseModel>> ImportIdeasCsvAsync(
        WorkflowActorContext actor,
        Guid boardId,
        byte[] fileBytes,
        CancellationToken cancellationToken)
    {
        var board = await _dataAccess.FindBoardByIdAsync(boardId, cancellationToken);
        if (board is null)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var authorization = await AuthorizeAsync(actor, board.OrganizationId, ManageRoles, cancellationToken);
        if (!authorization.Succeeded)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(authorization.FailureReason!.Value);
        }

        if (fileBytes.Length == 0)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "CSV file is required." }
                });
        }

        string csvText;
        try
        {
            csvText = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(fileBytes);
        }
        catch (DecoderFallbackException)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "CSV file must be UTF-8 encoded." }
                });
        }

        using var reader = new StringReader(csvText);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "CSV header row is required." }
                });
        }

        var header = ParseCsvLine(headerLine.TrimStart('\uFEFF'));
        var expectedHeader = new[] { "Title", "Description", "Priority", "IdeaType", "BusinessImpact", "DueDate", "Status", "AssignedTo", "Tags" };
        if (header.Count != expectedHeader.Length || !header.SequenceEqual(expectedHeader, StringComparer.Ordinal))
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "CSV header must be exactly: Title,Description,Priority,IdeaType,BusinessImpact,DueDate,Status,AssignedTo,Tags" }
                });
        }

        var swimlanes = await _dataAccess.ListBoardSwimlanesAsync(boardId, cancellationToken);
        var activeSwimlanes = swimlanes
            .Where(item => !item.Status.IsDeleted)
            .OrderBy(item => item.Order)
            .ToArray();

        if (activeSwimlanes.Length == 0)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "The target board has no active swimlanes." }
                });
        }

        var statuses = await _dataAccess.ListStatusesAsync(board.OrganizationId, cancellationToken);
        var statusByName = statuses
            .Where(item => !item.IsDeleted)
            .GroupBy(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(item => item.Key, item => item.First(), StringComparer.OrdinalIgnoreCase);

        var activeIdeaTypes = (await _dataAccess.ListIdeaTypesAsync(board.OrganizationId, cancellationToken))
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.SortOrder)
            .ToArray();
        var activeBusinessImpacts = (await _dataAccess.ListBusinessImpactsAsync(board.OrganizationId, cancellationToken))
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.SortOrder)
            .ToArray();

        if (activeIdeaTypes.Length == 0 || activeBusinessImpacts.Length == 0)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "The organization must have active Idea Type and Business Impact options." }
                });
        }

        var ideaTypeByName = activeIdeaTypes.ToDictionary(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase);
        var businessImpactByName = activeBusinessImpacts.ToDictionary(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase);

        var existingIdeas = await _dataAccess.ListIdeasByBoardIdAsync(boardId, cancellationToken);
        var existingTitles = existingIdeas
            .Where(item => !item.IsDeleted)
            .Select(item => item.Title.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rowErrors = new Dictionary<string, List<string>>();
        var candidates = new List<IdeaImportCandidate>();
        var seenTitlesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skippedCount = 0;
        var dataRowNumber = 0;

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            dataRowNumber++;
            if (dataRowNumber > 500)
            {
                return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                    WorkflowFailureReason.ValidationError,
                    new Dictionary<string, string[]>
                    {
                        ["file"] = new[] { "CSV file cannot exceed 500 data rows." }
                    });
            }

            var columns = ParseCsvLine(line);
            if (columns.Count != expectedHeader.Length)
            {
                AddRowError(rowErrors, dataRowNumber, "Row must contain exactly 9 columns.");
                continue;
            }

            var title = columns[0].Trim();
            var description = columns[1].Trim();
            var priorityText = columns[2].Trim();
            var ideaTypeText = columns[3].Trim();
            var businessImpactText = columns[4].Trim();
            var dueDateText = columns[5].Trim();
            var statusText = columns[6].Trim();
            var assignedToText = columns[7].Trim();
            var tagsText = columns[8].Trim();

            if (title.Length == 0)
            {
                AddRowError(rowErrors, dataRowNumber, "Title is required.");
            }
            else if (title.Length > 150)
            {
                AddRowError(rowErrors, dataRowNumber, "Title must be 150 characters or fewer.");
            }

            if (description.Length == 0)
            {
                AddRowError(rowErrors, dataRowNumber, "Description is required.");
            }
            else if (description.Length > 4000)
            {
                AddRowError(rowErrors, dataRowNumber, "Description must be 4000 characters or fewer.");
            }

            if (!Enum.TryParse<IdeaPriority>(priorityText, ignoreCase: true, out var priority))
            {
                AddRowError(rowErrors, dataRowNumber, "Priority must be one of: Low, Medium, High, Critical.");
            }

            DateOnly? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dueDateText))
            {
                if (!DateOnly.TryParseExact(dueDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDueDate))
                {
                    AddRowError(rowErrors, dataRowNumber, "DueDate must be a valid YYYY-MM-DD date.");
                }
                else
                {
                    dueDate = parsedDueDate;
                }
            }

            if (!string.IsNullOrWhiteSpace(title) && !seenTitlesInFile.Add(title))
            {
                AddRowError(rowErrors, dataRowNumber, "Title is duplicated within the import file.");
            }

            if (!string.IsNullOrWhiteSpace(title) && existingTitles.Contains(title))
            {
                skippedCount++;
                continue;
            }

            Guid statusId;
            if (string.IsNullOrWhiteSpace(statusText))
            {
                statusId = activeSwimlanes[0].StatusId;
            }
            else if (statusByName.TryGetValue(statusText, out var resolvedStatus))
            {
                statusId = resolvedStatus.Id;
            }
            else
            {
                AddRowError(rowErrors, dataRowNumber, $"Status '{statusText}' does not exist in this organization.");
                statusId = Guid.Empty;
            }

            var ideaTypeId = activeIdeaTypes[0].Id;
            if (!string.IsNullOrWhiteSpace(ideaTypeText))
            {
                if (ideaTypeByName.TryGetValue(ideaTypeText, out var ideaType))
                {
                    ideaTypeId = ideaType.Id;
                }
                else
                {
                    AddRowError(rowErrors, dataRowNumber, $"IdeaType '{ideaTypeText}' is not an active option in this organization.");
                }
            }

            var businessImpactId = activeBusinessImpacts[0].Id;
            if (!string.IsNullOrWhiteSpace(businessImpactText))
            {
                if (businessImpactByName.TryGetValue(businessImpactText, out var businessImpact))
                {
                    businessImpactId = businessImpact.Id;
                }
                else
                {
                    AddRowError(rowErrors, dataRowNumber, $"BusinessImpact '{businessImpactText}' is not an active option in this organization.");
                }
            }

            var assigneeUserIds = new List<Guid>();
            if (!string.IsNullOrWhiteSpace(assignedToText))
            {
                var assigneeEmails = assignedToText.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (assigneeEmails.Length > 5)
                {
                    AddRowError(rowErrors, dataRowNumber, "AssignedTo cannot contain more than five email addresses.");
                }

                if (assigneeEmails.Distinct(StringComparer.OrdinalIgnoreCase).Count() != assigneeEmails.Length)
                {
                    AddRowError(rowErrors, dataRowNumber, "AssignedTo email addresses must be distinct.");
                }

                foreach (var assigneeEmail in assigneeEmails.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var assignee = await _dataAccess.FindUserByEmailAsync(board.OrganizationId, assigneeEmail, cancellationToken);
                    if (assignee is null || assignee.Status != UserLifecycleStatus.Active)
                    {
                        AddRowError(rowErrors, dataRowNumber, $"AssignedTo '{assigneeEmail}' must resolve to an active user in this organization.");
                    }
                    else
                    {
                        assigneeUserIds.Add(assignee.Id);
                    }
                }
            }

            var tagNames = tagsText.Length == 0
                ? Array.Empty<string>()
                : tagsText.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            if (tagNames.Length > 10)
            {
                AddRowError(rowErrors, dataRowNumber, "Tags cannot contain more than 10 distinct values.");
            }

            foreach (var tagName in tagNames)
            {
                if (tagName.Length > 100)
                {
                    AddRowError(rowErrors, dataRowNumber, "Each tag must be 100 characters or fewer.");
                    break;
                }
            }

            candidates.Add(new IdeaImportCandidate(
                dataRowNumber,
                title,
                description,
                priorityText,
                dueDate,
                statusId,
                ideaTypeId,
                businessImpactId,
                assigneeUserIds,
                tagNames));
        }

        if (dataRowNumber == 0)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                new Dictionary<string, string[]>
                {
                    ["file"] = new[] { "CSV file must include at least one non-blank data row." }
                });
        }

        if (rowErrors.Count > 0)
        {
            return WorkflowImportResult<IdeaImportResponseModel>.Failure(
                WorkflowFailureReason.ValidationError,
                rowErrors.ToDictionary(item => item.Key, item => item.Value.ToArray()));
        }

        var createdIdeas = new List<Idea>(candidates.Count);
        var stagedTags = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
        var nowUtc = DateTime.UtcNow;

        foreach (var candidate in candidates)
        {
            var idea = new Idea
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                OrganizationId = board.OrganizationId,
                AuthorUserId = actor.UserId,
                Title = candidate.Title,
                Description = candidate.Description,
                Priority = Enum.Parse<IdeaPriority>(candidate.Priority, true),
                IdeaTypeId = candidate.IdeaTypeId,
                BusinessImpactId = candidate.BusinessImpactId,
                DueDate = candidate.DueDate,
                StatusId = candidate.StatusId,
                CreatedAtUtc = nowUtc
            };

            _dataAccess.AddIdea(idea);

            foreach (var assigneeUserId in candidate.AssigneeUserIds)
            {
                _dataAccess.AddIdeaAssignee(new IdeaAssignee
                {
                    IdeaId = idea.Id,
                    UserId = assigneeUserId
                });
            }

            foreach (var rawTag in candidate.TagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedTag = rawTag.Trim().ToUpperInvariant();
                if (normalizedTag.Length == 0)
                {
                    continue;
                }

                if (!stagedTags.TryGetValue(normalizedTag, out var tag))
                {
                    tag = await _dataAccess.FindTagByNormalizedNameAsync(board.OrganizationId, normalizedTag, cancellationToken);
                    if (tag is null)
                    {
                        tag = new Tag
                        {
                            Id = Guid.NewGuid(),
                            OrganizationId = board.OrganizationId,
                            Name = rawTag.Trim(),
                            NormalizedName = normalizedTag
                        };

                        _dataAccess.AddTag(tag);
                    }

                    stagedTags[normalizedTag] = tag;
                }

                _dataAccess.AddIdeaTag(new IdeaTag
                {
                    IdeaId = idea.Id,
                    TagId = tag.Id
                });
            }

            createdIdeas.Add(idea);
        }

        await _dataAccess.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteIdeasImportedAsync(
            actor.UserId,
            board.OrganizationId,
            board.Id,
            createdIdeas.Count,
            skippedCount,
            cancellationToken);

        foreach (var idea in createdIdeas)
        {
            await _auditWriter.WriteIdeaCreatedAsync(actor.UserId, idea, cancellationToken);
        }

        return WorkflowImportResult<IdeaImportResponseModel>.Success(new IdeaImportResponseModel
        {
            ImportedCount = createdIdeas.Count,
            SkippedCount = skippedCount,
            Errors = Array.Empty<string>()
        });
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

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, CollaborateRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var board = await _dataAccess.FindBoardByIdAsync(idea.BoardId, cancellationToken);

        if (board is null)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.BoardNotFound);
        }

        var existingAssigneeUserIds = idea.Assignees.Select(item => item.UserId).ToHashSet();
        var validationErrors = await ValidateIdeaWriteRequestAsync(board, request, cancellationToken, existingAssigneeUserIds);

        if (validationErrors.Count > 0)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, validationErrors);
        }

        var assignmentsChanged = !existingAssigneeUserIds.OrderBy(item => item)
            .SequenceEqual(request.AssigneeUserIds.OrderBy(item => item));
        var descriptionChanged = !string.Equals(idea.Description, request.Description.Trim(), StringComparison.Ordinal);

        if ((assignmentsChanged || descriptionChanged) && !CanManageIdeaContent(actor, idea))
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.Forbidden);
        }

        var selectedStatusId = await ResolveTargetStatusIdAsync(board.Id, request.StatusId ?? idea.StatusId, cancellationToken);

        if (!selectedStatusId.HasValue)
        {
            return WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.ValidationError, new[] { "A valid board status is required." });
        }

        idea.Title = request.Title.Trim();
        idea.Description = request.Description.Trim();
        idea.Priority = Enum.Parse<IdeaPriority>(request.Priority, true);
        idea.IdeaTypeId = request.IdeaTypeId;
        idea.BusinessImpactId = request.BusinessImpactId;
        idea.DueDate = request.DueDate;
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
        await ReplaceIdeaAssigneesAsync(idea, request.AssigneeUserIds, cancellationToken);
        await ReplaceIdeaTagsAndMentionsAsync(idea, actor.UserId, request.TagNames, mentionResolution.ResolvedEmails, cancellationToken);
        await _dataAccess.SaveChangesAsync(cancellationToken);

        var persisted = await _dataAccess.FindIdeaByIdAsync(idea.Id, cancellationToken);
        await _auditWriter.WriteIdeaUpdatedAsync(actor.UserId, persisted!, cancellationToken);
        return WorkflowResult<IdeaDetailModel>.Success(ToIdeaDetail(persisted!, actor.UserId));
    }

    public async Task<WorkflowResult> SoftDeleteIdeaAsync(
        WorkflowActorContext actor,
        Guid ideaId,
        CancellationToken cancellationToken)
    {
        var idea = await _dataAccess.FindIdeaByIdAsync(ideaId, cancellationToken);

        if (idea is null)
        {
            return WorkflowResult.Failure(WorkflowFailureReason.IdeaNotFound);
        }

        var authorization = await AuthorizeAsync(actor, idea.OrganizationId, ManageRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        idea.IsDeleted = true;
        idea.DeletedAtUtc = DateTime.UtcNow;
        idea.DeletedByUserId = actor.UserId;
        idea.UpdatedAtUtc = idea.DeletedAtUtc;
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteIdeaDeletedAsync(actor.UserId, idea, cancellationToken);
        return WorkflowResult.Success();
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
            await NotifyIdeaParticipantsAsync(idea, actor.UserId, NotificationEventType.IdeaStatusChanged, cancellationToken);

            return WorkflowResult.Success();
        }

        var previousStatusIdForMove = idea.StatusId;
        idea.StatusId = statusId.Value;
        idea.UpdatedAtUtc = now;
        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteIdeaStatusMovedAsync(actor.UserId, idea, previousStatusIdForMove, cancellationToken);

        await NotifyIdeaParticipantsAsync(idea, actor.UserId, NotificationEventType.IdeaStatusChanged, cancellationToken);

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

        await NotifyIdeaParticipantsAsync(idea, actor.UserId, NotificationEventType.CommentAdded, cancellationToken);

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

    public async Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>> ListMyIdeasAsync(
        WorkflowActorContext actor,
        Guid organizationId,
        OrgIdeaListQueryModel query,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(actor, organizationId, ReadRoles, cancellationToken);

        if (!authorization.Succeeded)
        {
            return WorkflowResult<PagedResultModel<IdeaListItemModel>>.Failure(authorization.FailureReason!.Value, authorization.Errors);
        }

        var filter = query.Filter?.Trim().ToLowerInvariant() ?? OrgIdeaFilter.All;
        Guid? authorUserId = string.Equals(filter, OrgIdeaFilter.CreatedByMe, StringComparison.OrdinalIgnoreCase)
            ? actor.UserId
            : null;
        Guid? assigneeUserId = string.Equals(filter, OrgIdeaFilter.AssignedToMe, StringComparison.OrdinalIgnoreCase)
            ? actor.UserId
            : null;

        var pageSize = Math.Clamp(query.PageSize, 1, 250);
        var page = Math.Max(1, query.Page);

        var (items, totalCount) = await _dataAccess.ListIdeasByOrgAsync(
            organizationId,
            authorUserId,
            assigneeUserId,
            query.Search,
            page,
            pageSize,
            cancellationToken);

        var result = new PagedResultModel<IdeaListItemModel>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(item => ToIdeaListItem(item, actor.UserId)).ToArray()
        };

        return WorkflowResult<PagedResultModel<IdeaListItemModel>>.Success(result);
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

    private static IdeaListItemModel ToIdeaListItem(Idea idea, Guid actorUserId)
    {
        return new IdeaListItemModel
        {
            IdeaId = idea.Id,
            BoardId = idea.BoardId,
            Title = idea.Title,
            Priority = idea.Priority.ToString(),
            IdeaTypeId = idea.IdeaTypeId,
            IdeaTypeName = idea.IdeaType.Name,
            BusinessImpactId = idea.BusinessImpactId,
            BusinessImpactName = idea.BusinessImpact.Name,
            BusinessImpactColor = idea.BusinessImpact.Color,
            DueDate = idea.DueDate,
            Assignees = ToAssigneeSummaries(idea),
            TagNames = idea.IdeaTags.Select(item => item.Tag.Name).OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToArray(),
            StatusId = idea.StatusId,
            StatusName = idea.Status.Name,
            UpvoteCount = idea.Upvotes.Count,
            HasUpvoted = idea.Upvotes.Any(item => item.UserId == actorUserId),
            CommentCount = idea.Comments.Count,
            AuthorUserId = idea.AuthorUserId,
            AuthorDisplayName = $"{idea.AuthorUser.FirstName} {idea.AuthorUser.LastName}".Trim(),
            CreatedAtUtc = idea.CreatedAtUtc
        };
    }

    private static IdeaDetailModel ToIdeaDetail(Idea idea, Guid actorUserId)
    {
        return new IdeaDetailModel
        {
            IdeaId = idea.Id,
            BoardId = idea.BoardId,
            Title = idea.Title,
            Description = idea.Description,
            Priority = idea.Priority.ToString(),
            IdeaTypeId = idea.IdeaTypeId,
            IdeaTypeName = idea.IdeaType.Name,
            BusinessImpactId = idea.BusinessImpactId,
            BusinessImpactName = idea.BusinessImpact.Name,
            BusinessImpactColor = idea.BusinessImpact.Color,
            DueDate = idea.DueDate,
            Assignees = ToAssigneeSummaries(idea),
            StatusId = idea.StatusId,
            StatusName = idea.Status.Name,
            ApprovalState = idea.ApprovalState.ToString(),
            PendingApprovalTargetStatusId = idea.PendingApprovalTargetStatusId,
            PendingApprovalPreviousStatusId = idea.PendingApprovalPreviousStatusId,
            PendingApprovalRequestedAtUtc = idea.PendingApprovalRequestedAtUtc,
            PendingApprovalExpiresAtUtc = idea.PendingApprovalExpiresAtUtc,
            TagNames = idea.IdeaTags
                .Select(item => item.Tag.Name)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Mentions = idea.Mentions
                .Select(item => item.MentionedUser.Email)
                .OrderBy(item => item)
                .ToArray(),
            Comments = idea.Comments
                .OrderBy(item => item.CreatedAtUtc)
                .Select(ToCommentModel)
                .ToArray(),
            UpvoteCount = idea.Upvotes.Count,
            HasUpvoted = idea.Upvotes.Any(item => item.UserId == actorUserId),
            CommentCount = idea.Comments.Count
        };
    }

    private static IReadOnlyList<IdeaAssigneeSummaryModel> ToAssigneeSummaries(Idea idea)
    {
        return idea.Assignees
            .Select(item => item.User)
            .OrderBy(item => item.FirstName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.LastName, StringComparer.OrdinalIgnoreCase)
            .Select(item => new IdeaAssigneeSummaryModel
            {
                UserId = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                DisplayName = $"{item.FirstName} {item.LastName}".Trim(),
                IsActive = item.Status == UserLifecycleStatus.Active
            })
            .ToArray();
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
        CancellationToken cancellationToken,
        IReadOnlySet<Guid>? existingAssigneeUserIds = null)
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

        if (request.IdeaTypeId == Guid.Empty)
        {
            errors.Add("Idea Type is required.");
        }
        else
        {
            var ideaType = await _dataAccess.FindIdeaTypeByIdAsync(request.IdeaTypeId, cancellationToken);
            if (ideaType is null || ideaType.OrganizationId != board.OrganizationId || ideaType.IsDeleted)
            {
                errors.Add("Idea Type must be an active option in the board organization.");
            }
        }

        if (request.BusinessImpactId == Guid.Empty)
        {
            errors.Add("Business Impact is required.");
        }
        else
        {
            var businessImpact = await _dataAccess.FindBusinessImpactByIdAsync(request.BusinessImpactId, cancellationToken);
            if (businessImpact is null || businessImpact.OrganizationId != board.OrganizationId || businessImpact.IsDeleted)
            {
                errors.Add("Business Impact must be an active option in the board organization.");
            }
        }

        if (request.AssigneeUserIds.Count > 5)
        {
            errors.Add("An idea can have no more than five assignees.");
        }

        if (request.AssigneeUserIds.Distinct().Count() != request.AssigneeUserIds.Count)
        {
            errors.Add("Assignee users must be distinct.");
        }

        foreach (var assigneeUserId in request.AssigneeUserIds.Distinct())
        {
            if (existingAssigneeUserIds?.Contains(assigneeUserId) == true)
            {
                continue;
            }

            var assignee = await _dataAccess.FindOrganizationUserByIdAsync(board.OrganizationId, assigneeUserId, cancellationToken);
            if (assignee is null || assignee.Status != UserLifecycleStatus.Active)
            {
                errors.Add("Assignee users must be active and belong to the board organization.");
                break;
            }
        }

        var mentionResolution = await ResolveMentionEmailsAsync(
            board.OrganizationId,
            $"{request.Title}\n{request.Description}",
            request.MentionEmails,
            cancellationToken);

        errors.AddRange(mentionResolution.Errors);

        var normalizedTagNames = request.TagNames
            .Select(item => item?.Trim() ?? string.Empty)
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedTagNames.Length > 10)
        {
            errors.Add("An idea can have no more than 10 tags.");
        }

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

    private Task ReplaceIdeaAssigneesAsync(
        Idea idea,
        IReadOnlyList<Guid> assigneeUserIds,
        CancellationToken cancellationToken)
    {
        _dataAccess.RemoveIdeaAssignees(idea.Assignees);

        foreach (var userId in assigneeUserIds.Distinct())
        {
            _dataAccess.AddIdeaAssignee(new IdeaAssignee
            {
                IdeaId = idea.Id,
                UserId = userId
            });
        }

        return Task.CompletedTask;
    }

    private async Task NotifyIdeaParticipantsAsync(
        Idea idea,
        Guid actorUserId,
        NotificationEventType eventType,
        CancellationToken cancellationToken)
    {
        var recipientIds = idea.Assignees
            .Select(item => item.UserId)
            .Append(idea.AuthorUserId)
            .Where(item => item != actorUserId)
            .Distinct()
            .ToArray();

        foreach (var recipientUserId in recipientIds)
        {
            await _notificationWriter.WriteAsync(recipientUserId, actorUserId, eventType,
                idea.Id, idea.Title, idea.OrganizationId, idea.BoardId, cancellationToken);
        }
    }

    private static bool CanManageIdeaContent(WorkflowActorContext actor, Idea idea)
    {
        return actor.UserId == idea.AuthorUserId ||
            string.Equals(actor.Role, UserRole.OrgAdmin.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(actor.Role, UserRole.SiteAdmin.ToString(), StringComparison.OrdinalIgnoreCase);
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

    private static void AddRowError(Dictionary<string, List<string>> errors, int rowNumber, string message)
    {
        var key = rowNumber.ToString(CultureInfo.InvariantCulture);
        if (!errors.TryGetValue(key, out var list))
        {
            list = new List<string>();
            errors[key] = list;
        }

        list.Add(message);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var columns = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                columns.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        columns.Add(current.ToString());
        return columns;
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

    private sealed record IdeaImportCandidate(
        int RowNumber,
        string Title,
        string Description,
        string Priority,
        DateOnly? DueDate,
        Guid StatusId,
        Guid IdeaTypeId,
        Guid BusinessImpactId,
        IReadOnlyList<Guid> AssigneeUserIds,
        IReadOnlyList<string> TagNames);

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
            IsDeleted = status.IsDeleted,
            Color = status.Color,
            SortOrder = status.SortOrder,
            IsDefault = status.IsDefault
        };
    }

    private static IdeaTypeSummaryModel ToIdeaTypeSummary(IdeaType ideaType)
    {
        return new IdeaTypeSummaryModel
        {
            IdeaTypeId = ideaType.Id,
            OrganizationId = ideaType.OrganizationId,
            Name = ideaType.Name,
            SortOrder = ideaType.SortOrder,
            IsDeleted = ideaType.IsDeleted
        };
    }

    private static BusinessImpactSummaryModel ToBusinessImpactSummary(BusinessImpact businessImpact)
    {
        return new BusinessImpactSummaryModel
        {
            BusinessImpactId = businessImpact.Id,
            OrganizationId = businessImpact.OrganizationId,
            Name = businessImpact.Name,
            Color = businessImpact.Color,
            SortOrder = businessImpact.SortOrder,
            IsDeleted = businessImpact.IsDeleted
        };
    }

    private static string? ValidateOptionName(string name, string optionType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return $"{optionType} name is required.";
        }

        return name.Length > 100
            ? $"{optionType} name must be 100 characters or fewer."
            : null;
    }

    private static bool IsCompleteReorder(IEnumerable<Guid> existingIds, IReadOnlyList<Guid> requestedIds)
    {
        var existingIdSet = existingIds.ToHashSet();

        return requestedIds.Count == existingIdSet.Count &&
            requestedIds.Distinct().Count() == requestedIds.Count &&
            requestedIds.All(existingIdSet.Contains);
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

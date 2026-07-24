using SargentNexus.Domain;

namespace SargentNexus.Application.Workflow;

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

    void AddStatus(Status status);

    void AddBoard(Board board);

    void AddBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

    void RemoveBoardSwimlanes(IEnumerable<BoardSwimlane> swimlanes);

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
}

public sealed class WorkflowManagementService : IWorkflowManagementService
{
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

    private readonly IWorkflowDataAccess _dataAccess;
    private readonly IWorkflowAuditWriter _auditWriter;

    public WorkflowManagementService(
        IWorkflowDataAccess dataAccess,
        IWorkflowAuditWriter auditWriter)
    {
        _dataAccess = dataAccess;
        _auditWriter = auditWriter;
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
            Name = request.Name.Trim()
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
        _dataAccess.AddBoardSwimlanes(newSwimlanes);

        await _dataAccess.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteBoardUpdatedAsync(actor.UserId, board, orderedStatusIds, previousName, cancellationToken);

        var statusesById = selectedStatuses.ToDictionary(item => item.Id);

        return WorkflowResult<BoardDetailModel>.Success(new BoardDetailModel
        {
            BoardId = board.Id,
            OrganizationId = board.OrganizationId,
            Name = board.Name,
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

    private static SwimlaneModel ToSwimlaneModel(BoardSwimlane swimlane)
    {
        return new SwimlaneModel
        {
            StatusId = swimlane.StatusId,
            StatusName = swimlane.Status.Name,
            IsDeletedStatus = swimlane.Status.IsDeleted,
            Order = swimlane.Order
        };
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

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

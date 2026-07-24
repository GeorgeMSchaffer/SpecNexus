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
}

internal sealed class FakeWorkflowAuditWriter : IWorkflowAuditWriter
{
    public List<Status> StatusCreatedEvents { get; } = new();

    public List<(Status Status, string PreviousName)> StatusUpdatedEvents { get; } = new();

    public List<Status> StatusDeletedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> StatusIds)> BoardCreatedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> StatusIds, string PreviousName)> BoardUpdatedEvents { get; } = new();

    public List<(Board Board, IReadOnlyList<Guid> OrderedStatusIds)> BoardReorderedEvents { get; } = new();

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
}

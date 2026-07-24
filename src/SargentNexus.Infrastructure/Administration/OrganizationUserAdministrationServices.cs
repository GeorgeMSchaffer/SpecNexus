using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SargentNexus.Application.Administration;
using SargentNexus.Domain;

namespace SargentNexus.Infrastructure.Administration;

internal sealed class OrganizationUserAdministrationStore : IOrganizationUserAdministrationStore
{
    private static readonly string[] DefaultStatusNames =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

    private readonly SargentNexusDbContext _dbContext;

    public OrganizationUserAdministrationStore(SargentNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return _dbContext.Organizations.SingleOrDefaultAsync(item => item.Id == organizationId, cancellationToken);
    }

    public async Task<PagedResultModel<OrganizationListItemModel>> GetOrganizationsAsync(
        string? search,
        bool includeArchived,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Organizations.AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(item => !item.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(item =>
                item.CompanyName.Contains(trimmedSearch) ||
                item.City.Contains(trimmedSearch) ||
                item.State.Contains(trimmedSearch));
        }

        query = ApplyOrganizationSort(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new OrganizationListItemModel
            {
                OrganizationId = item.Id,
                CompanyName = item.CompanyName,
                City = item.City,
                State = item.State,
                Phone = item.Phone,
                IsArchived = item.IsArchived,
                LogoThumbnailUrl = null
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResultModel<OrganizationListItemModel>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
    }

    public async Task<(Guid BoardId, int StatusCount)> AddOrganizationWithDefaultsAsync(Organization organization, CancellationToken cancellationToken)
    {
        _dbContext.Organizations.Add(organization);

        var statuses = DefaultStatusNames
            .Select((name, index) => new
            {
                Entity = new Status
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    Name = name,
                    IsDeleted = false
                },
                Order = index
            })
            .ToArray();

        _dbContext.Statuses.AddRange(statuses.Select(item => item.Entity));

        var board = new Board
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            Name = $"{organization.CompanyName} Board"
        };

        _dbContext.Boards.Add(board);
        _dbContext.BoardSwimlanes.AddRange(statuses.Select(item => new BoardSwimlane
        {
            BoardId = board.Id,
            StatusId = item.Entity.Id,
            Order = item.Order
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (board.Id, statuses.Length);
    }

    public async Task<Organization?> FindUserOrganizationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user?.OrganizationId is null)
        {
            return null;
        }

        return await _dbContext.Organizations.SingleOrDefaultAsync(item => item.Id == user.OrganizationId, cancellationToken);
    }

    public Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
    }

    public Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
    }

    public Task<int> CountOrgAdminsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.CountAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Role == UserRole.OrgAdmin &&
                item.Status == UserLifecycleStatus.Active,
            cancellationToken);
    }

    public async Task<PagedResultModel<UserSummaryModel>> GetUsersByOrganizationAsync(
        Guid organizationId,
        UserSearchFilters filters,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.Where(item => item.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var trimmedSearch = filters.Search.Trim();
            query = query.Where(item =>
                item.FirstName.Contains(trimmedSearch) ||
                item.LastName.Contains(trimmedSearch) ||
                item.Email.Contains(trimmedSearch));
        }

        if (filters.Role.HasValue)
        {
            query = query.Where(item => item.Role == filters.Role.Value);
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(item => item.Status == filters.Status.Value);
        }

        query = ApplyUserSort(query, filters.SortBy, filters.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(item => new UserSummaryModel
            {
                UserId = item.Id,
                OrganizationId = item.OrganizationId,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Email = item.Email,
                Role = item.Role.ToString(),
                Status = item.Status.ToString()
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResultModel<UserSummaryModel>
        {
            Items = items,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount,
            SortBy = filters.SortBy,
            SortDirection = filters.SortDirection
        };
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Organization> ApplyOrganizationSort(IQueryable<Organization> query, string sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(item => item.Id)
                : query.OrderBy(item => item.Id);
        }

        return descending
            ? query.OrderByDescending(item => item.CompanyName)
            : query.OrderBy(item => item.CompanyName);
    }

    private static IQueryable<User> ApplyUserSort(IQueryable<User> query, string sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(sortBy, "email", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(item => item.Email)
                : query.OrderBy(item => item.Email);
        }

        if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(item => item.Id)
                : query.OrderBy(item => item.Id);
        }

        return descending
            ? query.OrderByDescending(item => item.LastName).ThenByDescending(item => item.FirstName)
            : query.OrderBy(item => item.LastName).ThenBy(item => item.FirstName);
    }
}

internal sealed class OrganizationUserAuditWriter : IOrganizationUserAuditWriter
{
    private readonly SargentNexusDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public OrganizationUserAuditWriter(SargentNexusDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public Task WriteOrganizationCreatedAsync(
        Guid actorUserId,
        Organization organization,
        Guid defaultBoardId,
        int defaultStatusCount,
        CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organization.Id,
            "Organization",
            organization.Id,
            "Administration.OrganizationCreated",
            new
            {
                organization.CompanyName,
                DefaultBoardId = defaultBoardId,
                DefaultStatusCount = defaultStatusCount
            },
            cancellationToken);
    }

    public Task WriteOrganizationUpdatedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organization.Id,
            "Organization",
            organization.Id,
            "Administration.OrganizationUpdated",
            new
            {
                organization.CompanyName,
                organization.City,
                organization.State,
                organization.Phone
            },
            cancellationToken);
    }

    public Task WriteOrganizationArchivedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            organization.Id,
            "Organization",
            organization.Id,
            "Administration.OrganizationArchived",
            new
            {
                organization.CompanyName,
                organization.IsArchived
            },
            cancellationToken);
    }

    public Task WriteUserCreatedAsync(Guid actorUserId, User user, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            user.OrganizationId,
            "User",
            user.Id,
            "Administration.UserCreated",
            new
            {
                user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            },
            cancellationToken);
    }

    public Task WriteUserUpdatedAsync(
        Guid actorUserId,
        User user,
        string previousRole,
        string previousStatus,
        CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId,
            user.OrganizationId,
            "User",
            user.Id,
            "Administration.UserUpdated",
            new
            {
                user.Email,
                PreviousRole = previousRole,
                NewRole = user.Role.ToString(),
                PreviousStatus = previousStatus,
                NewStatus = user.Status.ToString()
            },
            cancellationToken);
    }

    private async Task WriteAsync(
        Guid actorUserId,
        Guid? organizationId,
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

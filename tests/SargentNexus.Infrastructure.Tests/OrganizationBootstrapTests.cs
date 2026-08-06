using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SargentNexus.Application.Administration;
using SargentNexus.Domain;
using SargentNexus.Infrastructure;

namespace SargentNexus.Infrastructure.Tests;

/// <summary>
/// T050: Infrastructure-level end-to-end coverage for organization bootstrap defaults
/// (default statuses, default board, swimlane ordering) and administration audit writer persistence.
/// </summary>
public sealed class OrganizationBootstrapTests
{
    private static readonly string[] ExpectedDefaultIdeaTypeNames =
    {
        "Continuous Improvement",
        "Process Revision"
    };

    private static readonly (string Name, string Color)[] ExpectedDefaultBusinessImpacts =
    {
        ("Low", "#16A34A"),
        ("Medium", "#2563EB"),
        ("High", "#D97706"),
        ("Critical", "#DC2626")
    };

    private static readonly string[] ExpectedDefaultStatusNames =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

    // ── Organization Bootstrap: AddOrganizationWithDefaultsAsync ──────────────

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_CreatesExactlyFiveDefaultStatuses()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var statuses = await dbContext.Statuses
            .Where(s => s.OrganizationId == organization.Id)
            .ToListAsync();

        Assert.Equal(5, statuses.Count);
        Assert.Equal(
            ExpectedDefaultStatusNames.OrderBy(n => n).ToArray(),
            statuses.Select(s => s.Name).OrderBy(n => n).ToArray());
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_AllDefaultStatusesAreNotDeleted()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var statuses = await dbContext.Statuses
            .Where(s => s.OrganizationId == organization.Id)
            .ToListAsync();

        Assert.All(statuses, s => Assert.False(s.IsDeleted));
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_CreatesDefaultBoardBelongingToOrganization()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        var (boardId, statusCount) = await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, boardId);
        Assert.Equal(5, statusCount);

        var board = await dbContext.Boards.SingleAsync(b => b.OrganizationId == organization.Id);
        Assert.Equal(boardId, board.Id);
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_CreatesDefaultBoardWithFiveSwimlanesOrderedZeroToFour()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        var (boardId, _) = await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var swimlanes = await dbContext.BoardSwimlanes
            .Where(sw => sw.BoardId == boardId)
            .OrderBy(sw => sw.Order)
            .ToListAsync();

        Assert.Equal(5, swimlanes.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, swimlanes.Select(sw => sw.Order).ToArray());
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_SwimlaneStatusIdsAllBelongToOrganization()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        var (boardId, _) = await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var orgStatusIds = (await dbContext.Statuses
            .Where(s => s.OrganizationId == organization.Id)
            .Select(s => s.Id)
            .ToListAsync()).ToHashSet();

        var swimlaneStatusIds = await dbContext.BoardSwimlanes
            .Where(sw => sw.BoardId == boardId)
            .Select(sw => sw.StatusId)
            .ToListAsync();

        Assert.Equal(orgStatusIds.Count, swimlaneStatusIds.Count);
        Assert.All(swimlaneStatusIds, statusId => Assert.Contains(statusId, orgStatusIds));
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_SwimlanesAreOrderedMatchingDefaultStatusSequence()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        var (boardId, _) = await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var statuses = await dbContext.Statuses
            .Where(s => s.OrganizationId == organization.Id)
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var orderedSwimlaneStatusNames = await dbContext.BoardSwimlanes
            .Where(sw => sw.BoardId == boardId)
            .OrderBy(sw => sw.Order)
            .Select(sw => sw.StatusId)
            .ToListAsync();

        var actualNames = orderedSwimlaneStatusNames.Select(id => statuses[id]).ToArray();
        Assert.Equal(ExpectedDefaultStatusNames, actualNames);
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_CreatesCanonicalIdeaTypesInSortOrder()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var ideaTypes = await dbContext.IdeaTypes
            .Where(item => item.OrganizationId == organization.Id)
            .OrderBy(item => item.SortOrder)
            .ToListAsync();

        Assert.Equal(ExpectedDefaultIdeaTypeNames, ideaTypes.Select(item => item.Name));
        Assert.Equal(new[] { 0, 1 }, ideaTypes.Select(item => item.SortOrder));
        Assert.All(ideaTypes, item => Assert.False(item.IsDeleted));
    }

    [Fact]
    public async Task AddOrganizationWithDefaultsAsync_CreatesCanonicalBusinessImpactsInSortOrder()
    {
        await using var dbContext = CreateDbContext();
        var store = CreateAdministrationStore(dbContext);
        var organization = CreateOrganization();

        await store.AddOrganizationWithDefaultsAsync(organization, CancellationToken.None);

        var businessImpacts = await dbContext.BusinessImpacts
            .Where(item => item.OrganizationId == organization.Id)
            .OrderBy(item => item.SortOrder)
            .ToListAsync();

        Assert.Equal(ExpectedDefaultBusinessImpacts.Select(item => item.Name), businessImpacts.Select(item => item.Name));
        Assert.Equal(ExpectedDefaultBusinessImpacts.Select(item => item.Color), businessImpacts.Select(item => item.Color));
        Assert.Equal(new[] { 0, 1, 2, 3 }, businessImpacts.Select(item => item.SortOrder));
        Assert.All(businessImpacts, item => Assert.False(item.IsDeleted));
    }

    // ── Administration Audit Writer Persistence ────────────────────────────────

    [Fact]
    public async Task OrganizationUserAuditWriter_WriteOrganizationCreatedAsync_PersistsExpectedAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
        var writer = CreateAuditWriter(dbContext, new FixedTimeProvider(nowUtc));

        var actorUserId = Guid.NewGuid();
        var organization = CreateOrganization();
        var defaultBoardId = Guid.NewGuid();
        const int defaultStatusCount = 5;

        await writer.WriteOrganizationCreatedAsync(actorUserId, organization, defaultBoardId, defaultStatusCount, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Administration.OrganizationCreated", auditEvent.EventType);
        Assert.Equal("Organization", auditEvent.EntityType);
        Assert.Equal(organization.Id, auditEvent.EntityId);
        Assert.Equal(organization.Id, auditEvent.OrganizationId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.Equal(organization.CompanyName, metadata.RootElement.GetProperty("CompanyName").GetString());
        Assert.Equal(defaultBoardId, metadata.RootElement.GetProperty("DefaultBoardId").GetGuid());
        Assert.Equal(defaultStatusCount, metadata.RootElement.GetProperty("DefaultStatusCount").GetInt32());
    }

    [Fact]
    public async Task OrganizationUserAuditWriter_WriteUserCreatedAsync_PersistsExpectedAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
        var writer = CreateAuditWriter(dbContext, new FixedTimeProvider(nowUtc));

        var actorUserId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            FirstName = "Alice",
            LastName = "Doe",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Status = UserLifecycleStatus.Active,
            MustChangePassword = true
        };

        await writer.WriteUserCreatedAsync(actorUserId, user, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Administration.UserCreated", auditEvent.EventType);
        Assert.Equal("User", auditEvent.EntityType);
        Assert.Equal(user.Id, auditEvent.EntityId);
        Assert.Equal(organizationId, auditEvent.OrganizationId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.Equal(user.Email, metadata.RootElement.GetProperty("Email").GetString());
        Assert.Equal("User", metadata.RootElement.GetProperty("Role").GetString());
        Assert.Equal("Active", metadata.RootElement.GetProperty("Status").GetString());
    }

    [Fact]
    public async Task OrganizationUserAuditWriter_WriteUserUpdatedAsync_PersistsExpectedAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
        var writer = CreateAuditWriter(dbContext, new FixedTimeProvider(nowUtc));

        var actorUserId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob@example.com",
            PasswordHash = "hash",
            Role = UserRole.OrgAdmin,
            Status = UserLifecycleStatus.Active,
            MustChangePassword = false
        };
        const string previousRole = "User";
        const string previousStatus = "Inactive";

        await writer.WriteUserUpdatedAsync(actorUserId, user, previousRole, previousStatus, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Administration.UserUpdated", auditEvent.EventType);
        Assert.Equal("User", auditEvent.EntityType);
        Assert.Equal(user.Id, auditEvent.EntityId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.Equal(user.Email, metadata.RootElement.GetProperty("Email").GetString());
        Assert.Equal(previousRole, metadata.RootElement.GetProperty("PreviousRole").GetString());
        Assert.Equal("OrgAdmin", metadata.RootElement.GetProperty("NewRole").GetString());
        Assert.Equal(previousStatus, metadata.RootElement.GetProperty("PreviousStatus").GetString());
        Assert.Equal("Active", metadata.RootElement.GetProperty("NewStatus").GetString());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SargentNexusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SargentNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SargentNexusDbContext(options);
    }

    private static Organization CreateOrganization() => new()
    {
        Id = Guid.NewGuid(),
        CompanyName = "Test Organization",
        Address = "123 Test St",
        City = "Testville",
        State = "TS",
        Zip = "00000",
        Phone = "555-555-5555",
        PrimaryContactFirstName = "Test",
        PrimaryContactLastName = "User",
        InviteCode = "TESTCODE",
        InviteCodeGeneratedAtUtc = DateTime.UtcNow,
        IsArchived = false
    };

    private static IOrganizationUserAdministrationStore CreateAdministrationStore(SargentNexusDbContext dbContext)
    {
        var assembly = typeof(SargentNexusDbContext).Assembly;
        var type = assembly.GetType("SargentNexus.Infrastructure.Administration.OrganizationUserAdministrationStore", throwOnError: true)!;
        return (IOrganizationUserAdministrationStore)(Activator.CreateInstance(type, dbContext)
            ?? throw new InvalidOperationException("Could not construct OrganizationUserAdministrationStore."));
    }

    private static IOrganizationUserAuditWriter CreateAuditWriter(SargentNexusDbContext dbContext, TimeProvider timeProvider)
    {
        var assembly = typeof(SargentNexusDbContext).Assembly;
        var type = assembly.GetType("SargentNexus.Infrastructure.Administration.OrganizationUserAuditWriter", throwOnError: true)!;
        return (IOrganizationUserAuditWriter)(Activator.CreateInstance(type, dbContext, timeProvider)
            ?? throw new InvalidOperationException("Could not construct OrganizationUserAuditWriter."));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(utcNow);
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}

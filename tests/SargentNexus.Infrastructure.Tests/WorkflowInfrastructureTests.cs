using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SargentNexus.Application.Workflow;
using SargentNexus.Domain;
using SargentNexus.Infrastructure;
using SargentNexus.Infrastructure.Workflow;

namespace SargentNexus.Infrastructure.Tests;

public sealed class WorkflowInfrastructureTests
{
    [Fact]
    public async Task WorkflowDataAccess_FindStatusesByIdsAsync_ExcludesDeletedAndForeignStatuses()
    {
        await using var dbContext = CreateDbContext();
        var orgOneId = Guid.NewGuid();
        var orgTwoId = Guid.NewGuid();

        var active = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgOneId,
            Name = "Active",
            IsDeleted = false
        };

        var deleted = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgOneId,
            Name = "Deleted",
            IsDeleted = true
        };

        var foreign = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgTwoId,
            Name = "Foreign",
            IsDeleted = false
        };

        dbContext.Statuses.AddRange(active, deleted, foreign);
        await dbContext.SaveChangesAsync();

        var dataAccess = new WorkflowDataAccess(dbContext);

        var statuses = await dataAccess.FindStatusesByIdsAsync(
            orgOneId,
            new[] { active.Id, deleted.Id, foreign.Id },
            CancellationToken.None);

        Assert.Single(statuses);
        Assert.Equal(active.Id, statuses[0].Id);
    }

    [Fact]
    public async Task WorkflowDataAccess_IdeaQueries_ExcludeDeletedAndAssignedToMeUsesAssigneeJoin()
    {
        await using var dbContext = CreateDbContext();
        var organizationId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var authorUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var ideaTypeId = Guid.NewGuid();
        var businessImpactId = Guid.NewGuid();
        var activeIdeaId = Guid.NewGuid();
        var deletedIdeaId = Guid.NewGuid();

        dbContext.Organizations.Add(new Organization { Id = organizationId, CompanyName = "Organization" });
        dbContext.Users.AddRange(
            new User
            {
                Id = authorUserId,
                OrganizationId = organizationId,
                Email = "author@test.local",
                FirstName = "Author",
                LastName = "User",
                PasswordHash = "hash",
                Role = UserRole.User,
                Status = UserLifecycleStatus.Active
            },
            new User
            {
                Id = assigneeUserId,
                OrganizationId = organizationId,
                Email = "assignee@test.local",
                FirstName = "Assigned",
                LastName = "User",
                PasswordHash = "hash",
                Role = UserRole.User,
                Status = UserLifecycleStatus.Active
            });
        dbContext.Statuses.Add(new Status { Id = statusId, OrganizationId = organizationId, Name = "New" });
        dbContext.IdeaTypes.Add(new IdeaType { Id = ideaTypeId, OrganizationId = organizationId, Name = "Type" });
        dbContext.BusinessImpacts.Add(new BusinessImpact
        {
            Id = businessImpactId,
            OrganizationId = organizationId,
            Name = "Impact",
            Color = "#123456"
        });
        dbContext.Boards.Add(new Board { Id = boardId, OrganizationId = organizationId, Name = "Board" });
        dbContext.Ideas.AddRange(
            new Idea
            {
                Id = activeIdeaId,
                OrganizationId = organizationId,
                BoardId = boardId,
                AuthorUserId = authorUserId,
                Title = "Active idea",
                Description = "Visible",
                Priority = IdeaPriority.Medium,
                StatusId = statusId,
                IdeaTypeId = ideaTypeId,
                BusinessImpactId = businessImpactId,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Idea
            {
                Id = deletedIdeaId,
                OrganizationId = organizationId,
                BoardId = boardId,
                AuthorUserId = authorUserId,
                Title = "Deleted idea",
                Description = "Hidden",
                Priority = IdeaPriority.Medium,
                StatusId = statusId,
                IdeaTypeId = ideaTypeId,
                BusinessImpactId = businessImpactId,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = true
            });
        dbContext.IdeaAssignees.Add(new IdeaAssignee { IdeaId = activeIdeaId, UserId = assigneeUserId });
        await dbContext.SaveChangesAsync();

        var dataAccess = new WorkflowDataAccess(dbContext);

        Assert.NotNull(await dataAccess.FindIdeaByIdAsync(activeIdeaId, CancellationToken.None));
        Assert.Null(await dataAccess.FindIdeaByIdAsync(deletedIdeaId, CancellationToken.None));
        var boardIdeas = await dataAccess.ListIdeasByBoardIdAsync(boardId, CancellationToken.None);
        Assert.Equal(activeIdeaId, Assert.Single(boardIdeas).Id);
        var (assignedIdeas, totalCount) = await dataAccess.ListIdeasByOrgAsync(
            organizationId,
            null,
            assigneeUserId,
            null,
            1,
            20,
            CancellationToken.None);
        Assert.Equal(1, totalCount);
        Assert.Equal(activeIdeaId, Assert.Single(assignedIdeas).Id);
    }

    [Fact]
    public async Task WorkflowAuditWriter_WriteBoardSwimlanesReorderedAsync_PersistsAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(nowUtc);
        var writer = new WorkflowAuditWriter(dbContext, timeProvider);

        var board = new Board
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Name = "Delivery"
        };
        var actorUserId = Guid.NewGuid();

        var orderedStatusIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        await writer.WriteBoardSwimlanesReorderedAsync(actorUserId, board, orderedStatusIds, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Workflow.BoardSwimlanesReordered", auditEvent.EventType);
        Assert.Equal("Board", auditEvent.EntityType);
        Assert.Equal(board.Id, auditEvent.EntityId);
        Assert.Equal(board.OrganizationId, auditEvent.OrganizationId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        var orderedIds = metadata.RootElement.GetProperty("OrderedStatusIds").EnumerateArray().Select(item => item.GetGuid()).ToArray();
        Assert.Equal(orderedStatusIds, orderedIds);
    }

    [Fact]
    public async Task WorkflowAuditWriter_WriteIdeaUpvoteToggledAsync_PersistsAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(nowUtc);
        var writer = new WorkflowAuditWriter(dbContext, timeProvider);

        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            BoardId = Guid.NewGuid(),
            AuthorUserId = Guid.NewGuid(),
            Title = "Audit idea",
            Description = "Audit description",
            Priority = IdeaPriority.Medium,
            StatusId = Guid.NewGuid(),
            CreatedAtUtc = nowUtc
        };
        var actorUserId = Guid.NewGuid();

        await writer.WriteIdeaUpvoteToggledAsync(actorUserId, idea, true, 3, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Workflow.IdeaUpvoteToggled", auditEvent.EventType);
        Assert.Equal("Idea", auditEvent.EntityType);
        Assert.Equal(idea.Id, auditEvent.EntityId);
        Assert.Equal(idea.OrganizationId, auditEvent.OrganizationId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.True(metadata.RootElement.GetProperty("HasUpvoted").GetBoolean());
        Assert.Equal(3, metadata.RootElement.GetProperty("UpvoteCount").GetInt32());
    }

    [Fact]
    public async Task WorkflowAuditWriter_WriteIdeaDeletedAsync_PersistsAuditEvent()
    {
        await using var dbContext = CreateDbContext();
        var nowUtc = new DateTime(2026, 8, 6, 14, 30, 0, DateTimeKind.Utc);
        var writer = new WorkflowAuditWriter(dbContext, new FixedTimeProvider(nowUtc));
        var actorUserId = Guid.NewGuid();
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            BoardId = Guid.NewGuid(),
            AuthorUserId = Guid.NewGuid(),
            Title = "Deleted idea",
            Description = "Audit deletion",
            Priority = IdeaPriority.Medium,
            StatusId = Guid.NewGuid(),
            CreatedAtUtc = nowUtc.AddDays(-1),
            IsDeleted = true,
            DeletedAtUtc = nowUtc,
            DeletedByUserId = actorUserId
        };

        await writer.WriteIdeaDeletedAsync(actorUserId, idea, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Workflow.IdeaDeleted", auditEvent.EventType);
        Assert.Equal("Idea", auditEvent.EntityType);
        Assert.Equal(idea.Id, auditEvent.EntityId);
        Assert.Equal(idea.OrganizationId, auditEvent.OrganizationId);
        Assert.Equal(actorUserId, auditEvent.ActorUserId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);

        using var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.True(metadata.RootElement.GetProperty("IsDeleted").GetBoolean());
        Assert.Equal(nowUtc, metadata.RootElement.GetProperty("DeletedAtUtc").GetDateTime());
        Assert.Equal(actorUserId, metadata.RootElement.GetProperty("DeletedByUserId").GetGuid());
    }

    private static SargentNexusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SargentNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new SargentNexusDbContext(options);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(utcNow);
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}

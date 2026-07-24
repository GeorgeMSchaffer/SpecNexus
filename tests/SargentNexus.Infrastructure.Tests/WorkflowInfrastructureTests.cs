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

        var orderedStatusIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        await writer.WriteBoardSwimlanesReorderedAsync(Guid.NewGuid(), board, orderedStatusIds, CancellationToken.None);

        var auditEvent = await dbContext.AuditEvents.SingleAsync();
        Assert.Equal("Workflow.BoardSwimlanesReordered", auditEvent.EventType);
        Assert.Equal("Board", auditEvent.EntityType);
        Assert.Equal(board.Id, auditEvent.EntityId);
        Assert.Equal(board.OrganizationId, auditEvent.OrganizationId);
        Assert.Equal(nowUtc, auditEvent.OccurredAtUtc);
        Assert.Contains(orderedStatusIds[0].ToString(), auditEvent.Metadata, StringComparison.Ordinal);
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

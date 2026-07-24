using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

public sealed class WorkflowManagementServiceTests
{
    [Fact]
    public async Task CreateBoard_WhenSwimlaneCountBelowTwo_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.CreateBoardAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new CreateBoardRequestModel
            {
                Name = "Single Lane",
                StatusIds = new[] { fixture.StatusOne.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("A board must have at least two swimlanes.", result.Errors);
    }

    [Fact]
    public async Task CreateBoard_WhenStatusOutsideOrganizationSubset_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var foreignStatus = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Name = "Foreign",
            IsDeleted = false
        };
        fixture.DataAccess.SeedStatus(foreignStatus);

        var result = await fixture.Service.CreateBoardAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new CreateBoardRequestModel
            {
                Name = "Subset Board",
                StatusIds = new[] { fixture.StatusOne.Id, foreignStatus.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Board statuses must exist in the organization and cannot be deleted.", result.Errors);
    }

    [Fact]
    public async Task ReorderSwimlanes_WhenValid_PersistsOrderImmediatelyAndAudits()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.ReorderSwimlanesAsync(
            fixture.OrgAdminActor,
            board.Id,
            new ReorderSwimlanesRequestModel
            {
                OrderedStatusIds = new[] { fixture.StatusTwo.Id, fixture.StatusOne.Id }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.BoardReorderedEvents);

        var swimlanes = await fixture.DataAccess.ListBoardSwimlanesAsync(board.Id, CancellationToken.None);
        Assert.Equal(fixture.StatusTwo.Id, swimlanes.Single(item => item.Order == 0).StatusId);
        Assert.Equal(fixture.StatusOne.Id, swimlanes.Single(item => item.Order == 1).StatusId);
    }

    [Fact]
    public async Task SoftDeleteStatus_WhenAuthorized_MarksDeletedAndWritesAudit()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.SoftDeleteStatusAsync(
            fixture.OrgAdminActor,
            fixture.StatusOne.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(fixture.StatusOne.IsDeleted);
        Assert.Equal(1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.StatusDeletedEvents);
    }

    [Fact]
    public async Task UpdateStatus_WhenRoleCannotManage_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.UpdateStatusAsync(
            fixture.ReadOnlyActor,
            fixture.StatusOne.Id,
            new UpdateStatusRequestModel { Name = "Updated" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    private sealed class WorkflowFixture
    {
        public WorkflowFixture()
        {
            Organization = new Organization
            {
                Id = Guid.NewGuid(),
                CompanyName = "Org",
                Address = "1 Main",
                City = "Austin",
                State = "TX",
                Zip = "78701",
                Phone = "512-555-0101",
                PrimaryContactFirstName = "A",
                PrimaryContactLastName = "B"
            };

            OrgAdminUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Email = "orgadmin@test.local",
                FirstName = "Org",
                LastName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.OrgAdmin,
                Status = UserLifecycleStatus.Active
            };

            ReadOnlyUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Email = "readonly@test.local",
                FirstName = "Read",
                LastName = "Only",
                PasswordHash = "hash",
                Role = UserRole.ReadOnly,
                Status = UserLifecycleStatus.Active
            };

            StatusOne = new Status
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "New / Pending",
                IsDeleted = false
            };

            StatusTwo = new Status
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "In Progress",
                IsDeleted = false
            };

            DataAccess = new FakeWorkflowDataAccess();
            AuditWriter = new FakeWorkflowAuditWriter();

            DataAccess.SeedOrganization(Organization);
            DataAccess.SeedUser(OrgAdminUser);
            DataAccess.SeedUser(ReadOnlyUser);
            DataAccess.SeedStatus(StatusOne);
            DataAccess.SeedStatus(StatusTwo);

            Service = new WorkflowManagementService(DataAccess, AuditWriter);

            OrgAdminActor = new WorkflowActorContext
            {
                UserId = OrgAdminUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.OrgAdmin.ToString()
            };

            ReadOnlyActor = new WorkflowActorContext
            {
                UserId = ReadOnlyUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.ReadOnly.ToString()
            };
        }

        public FakeWorkflowDataAccess DataAccess { get; }

        public FakeWorkflowAuditWriter AuditWriter { get; }

        public WorkflowManagementService Service { get; }

        public Organization Organization { get; }

        public User OrgAdminUser { get; }

        public User ReadOnlyUser { get; }

        public Status StatusOne { get; }

        public Status StatusTwo { get; }

        public WorkflowActorContext OrgAdminActor { get; }

        public WorkflowActorContext ReadOnlyActor { get; }

        public Board CreateBoardWithTwoSwimlanes()
        {
            var board = new Board
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "Delivery"
            };

            DataAccess.SeedBoard(board);
            DataAccess.SeedSwimlane(new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = StatusOne.Id,
                Order = 0
            });
            DataAccess.SeedSwimlane(new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = StatusTwo.Id,
                Order = 1
            });

            return board;
        }
    }
}

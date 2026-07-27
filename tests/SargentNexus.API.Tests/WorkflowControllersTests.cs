using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SargentNexus.API.Controllers;
using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.API.Tests;

public sealed class WorkflowControllersTests
{
    [Fact]
    public async Task StatusesCreate_WhenSucceeded_ReturnsCreated()
    {
        var model = new StatusSummaryModel
        {
            StatusId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Name = "In Review",
            IsDeleted = false
        };

        var service = new StubWorkflowManagementService
        {
            CreateStatusAsyncHandler = (_, _, _, _) => Task.FromResult(WorkflowResult<StatusSummaryModel>.Success(model))
        };

        var controller = CreateStatusesController(service, Guid.NewGuid(), UserRole.OrgAdmin.ToString(), Guid.NewGuid());

        var result = await controller.Create(Guid.NewGuid(), new CreateStatusRequestModel { Name = model.Name }, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal($"/api/v1/statuses/{model.StatusId}", created.Location);
        Assert.Same(model, created.Value);
    }

    [Fact]
    public async Task StatusesCreate_WhenUnauthorized_ReturnsUnauthorizedProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateStatusAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.Unauthorized))
        };

        var controller = CreateStatusesController(service, Guid.NewGuid(), UserRole.OrgAdmin.ToString(), Guid.NewGuid());

        var result = await controller.Create(Guid.NewGuid(), new CreateStatusRequestModel { Name = "Blocked" }, CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task BoardsCreate_WhenBelowTwoSwimlanes_ReturnsValidationProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateBoardAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<BoardDetailModel>.Failure(
                    WorkflowFailureReason.ValidationError,
                    new[] { "A board must have at least two swimlanes." }))
        };

        var controller = CreateBoardsController(service, Guid.NewGuid(), UserRole.OrgAdmin.ToString(), Guid.NewGuid());

        var result = await controller.Create(
            Guid.NewGuid(),
            new CreateBoardRequestModel
            {
                Name = "Delivery",
                StatusIds = new[] { Guid.NewGuid() }
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(validation.Errors.TryGetValue("workflow", out var errors));
        Assert.Contains("A board must have at least two swimlanes.", errors);
    }

    [Fact]
    public async Task BoardsCreate_WhenSucceeded_ReturnsCreated()
    {
        var model = new BoardDetailModel
        {
            BoardId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Name = "Delivery",
            Swimlanes = new[]
            {
                new SwimlaneModel
                {
                    StatusId = Guid.NewGuid(),
                    StatusName = "New / Pending",
                    IsDeletedStatus = false,
                    Order = 0
                },
                new SwimlaneModel
                {
                    StatusId = Guid.NewGuid(),
                    StatusName = "In Progress",
                    IsDeletedStatus = false,
                    Order = 1
                }
            }
        };

        var service = new StubWorkflowManagementService
        {
            CreateBoardAsyncHandler = (_, _, _, _) => Task.FromResult(WorkflowResult<BoardDetailModel>.Success(model))
        };

        var controller = CreateBoardsController(service, Guid.NewGuid(), UserRole.OrgAdmin.ToString(), Guid.NewGuid());

        var result = await controller.Create(
            Guid.NewGuid(),
            new CreateBoardRequestModel
            {
                Name = model.Name,
                StatusIds = model.Swimlanes.Select(item => item.StatusId).ToArray()
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal($"/api/v1/boards/{model.BoardId}", created.Location);
        Assert.Same(model, created.Value);
    }

    [Fact]
    public async Task BoardsReorder_WhenSucceeded_ReturnsNoContent()
    {
        var service = new StubWorkflowManagementService
        {
            ReorderSwimlanesAsyncHandler = (_, _, _, _) => Task.FromResult(WorkflowResult.Success())
        };

        var actorUserId = Guid.NewGuid();
        var actorOrgId = Guid.NewGuid();
        var controller = CreateBoardsController(service, actorUserId, UserRole.OrgAdmin.ToString(), actorOrgId);

        var boardId = Guid.NewGuid();
        var request = new ReorderSwimlanesRequestModel
        {
            OrderedStatusIds = new[] { Guid.NewGuid(), Guid.NewGuid() }
        };

        var result = await controller.Reorder(boardId, request, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);
        Assert.Equal(boardId, service.LastBoardId);
        Assert.Same(request, service.LastReorderRequest);
        Assert.Equal(actorUserId, service.LastActor?.UserId);
        Assert.Equal(actorOrgId, service.LastActor?.OrganizationId);
        Assert.Equal(UserRole.OrgAdmin.ToString(), service.LastActor?.Role);
    }

    private static StatusesController CreateStatusesController(
        IWorkflowManagementService service,
        Guid userId,
        string role,
        Guid? organizationId)
    {
        var controller = new StatusesController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.ControllerContext.HttpContext.User = CreatePrincipal(userId, role, organizationId);
        return controller;
    }

    private static BoardsController CreateBoardsController(
        IWorkflowManagementService service,
        Guid userId,
        string role,
        Guid? organizationId)
    {
        var controller = new BoardsController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.ControllerContext.HttpContext.User = CreatePrincipal(userId, role, organizationId);
        return controller;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId, string role, Guid? organizationId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        if (organizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", organizationId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static void AssertProblem(IActionResult result, int expectedStatus, string expectedTitle)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
    }

    private sealed class StubWorkflowManagementService : IWorkflowManagementService
    {
        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult<IReadOnlyList<StatusSummaryModel>>>>? ListStatusesAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CreateStatusRequestModel, CancellationToken, Task<WorkflowResult<StatusSummaryModel>>>? CreateStatusAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, UpdateStatusRequestModel, CancellationToken, Task<WorkflowResult<StatusSummaryModel>>>? UpdateStatusAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult>>? SoftDeleteStatusAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult<IReadOnlyList<BoardSummaryModel>>>>? ListBoardsAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult<BoardDetailModel>>>? GetBoardDetailAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CreateBoardRequestModel, CancellationToken, Task<WorkflowResult<BoardDetailModel>>>? CreateBoardAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, UpdateBoardRequestModel, CancellationToken, Task<WorkflowResult<BoardDetailModel>>>? UpdateBoardAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, ReorderSwimlanesRequestModel, CancellationToken, Task<WorkflowResult>>? ReorderSwimlanesAsyncHandler { get; init; }

        public WorkflowActorContext? LastActor { get; private set; }

        public Guid LastBoardId { get; private set; }

        public ReorderSwimlanesRequestModel? LastReorderRequest { get; private set; }

        public Task<WorkflowResult<IReadOnlyList<StatusSummaryModel>>> ListStatusesAsync(
            WorkflowActorContext actor,
            Guid organizationId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ListStatusesAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IReadOnlyList<StatusSummaryModel>>.Failure(WorkflowFailureReason.Forbidden))
                : ListStatusesAsyncHandler(actor, organizationId, cancellationToken);
        }

        public Task<WorkflowResult<StatusSummaryModel>> CreateStatusAsync(
            WorkflowActorContext actor,
            Guid organizationId,
            CreateStatusRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return CreateStatusAsyncHandler is null
                ? Task.FromResult(WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.Forbidden))
                : CreateStatusAsyncHandler(actor, organizationId, request, cancellationToken);
        }

        public Task<WorkflowResult<StatusSummaryModel>> UpdateStatusAsync(
            WorkflowActorContext actor,
            Guid statusId,
            UpdateStatusRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return UpdateStatusAsyncHandler is null
                ? Task.FromResult(WorkflowResult<StatusSummaryModel>.Failure(WorkflowFailureReason.Forbidden))
                : UpdateStatusAsyncHandler(actor, statusId, request, cancellationToken);
        }

        public Task<WorkflowResult> SoftDeleteStatusAsync(
            WorkflowActorContext actor,
            Guid statusId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return SoftDeleteStatusAsyncHandler is null
                ? Task.FromResult(WorkflowResult.Failure(WorkflowFailureReason.Forbidden))
                : SoftDeleteStatusAsyncHandler(actor, statusId, cancellationToken);
        }

        public Task<WorkflowResult<IReadOnlyList<BoardSummaryModel>>> ListBoardsAsync(
            WorkflowActorContext actor,
            Guid organizationId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ListBoardsAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IReadOnlyList<BoardSummaryModel>>.Failure(WorkflowFailureReason.Forbidden))
                : ListBoardsAsyncHandler(actor, organizationId, cancellationToken);
        }

        public Task<WorkflowResult<BoardDetailModel>> GetBoardDetailAsync(
            WorkflowActorContext actor,
            Guid boardId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return GetBoardDetailAsyncHandler is null
                ? Task.FromResult(WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : GetBoardDetailAsyncHandler(actor, boardId, cancellationToken);
        }

        public Task<WorkflowResult<BoardDetailModel>> CreateBoardAsync(
            WorkflowActorContext actor,
            Guid organizationId,
            CreateBoardRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return CreateBoardAsyncHandler is null
                ? Task.FromResult(WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : CreateBoardAsyncHandler(actor, organizationId, request, cancellationToken);
        }

        public Task<WorkflowResult<BoardDetailModel>> UpdateBoardAsync(
            WorkflowActorContext actor,
            Guid boardId,
            UpdateBoardRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return UpdateBoardAsyncHandler is null
                ? Task.FromResult(WorkflowResult<BoardDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : UpdateBoardAsyncHandler(actor, boardId, request, cancellationToken);
        }

        public Task<WorkflowResult> ReorderSwimlanesAsync(
            WorkflowActorContext actor,
            Guid boardId,
            ReorderSwimlanesRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            LastBoardId = boardId;
            LastReorderRequest = request;
            return ReorderSwimlanesAsyncHandler is null
                ? Task.FromResult(WorkflowResult.Failure(WorkflowFailureReason.Forbidden))
                : ReorderSwimlanesAsyncHandler(actor, boardId, request, cancellationToken);
        }
    }
}

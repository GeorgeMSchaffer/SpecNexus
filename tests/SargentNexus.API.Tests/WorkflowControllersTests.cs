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

    [Fact]
    public async Task IdeasCreate_WhenSucceeded_ReturnsCreated()
    {
        var model = new IdeaDetailModel
        {
            IdeaId = Guid.NewGuid(),
            BoardId = Guid.NewGuid(),
            Title = "Improve onboarding",
            Description = "Add guided setup",
            Priority = "High",
            StatusId = Guid.NewGuid(),
            StatusName = "New / Pending"
        };

        var service = new StubWorkflowManagementService
        {
            CreateIdeaAsyncHandler = (_, _, _, _) => Task.FromResult(WorkflowResult<IdeaDetailModel>.Success(model))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.CreateIdea(
            model.BoardId,
            new IdeaWriteRequestModel
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal($"/api/v1/ideas/{model.IdeaId}", created.Location);
        Assert.Same(model, created.Value);
    }

    [Fact]
    public async Task IdeasToggleUpvote_WhenSucceeded_ReturnsOk()
    {
        var ideaId = Guid.NewGuid();
        var response = new UpvoteToggleResultModel
        {
            IdeaId = ideaId,
            HasUpvoted = true,
            UpvoteCount = 5
        };

        var service = new StubWorkflowManagementService
        {
            ToggleIdeaUpvoteAsyncHandler = (_, _, _) => Task.FromResult(WorkflowResult<UpvoteToggleResultModel>.Success(response))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.ToggleUpvote(ideaId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task IdeasCreate_WhenValidationError_ReturnsValidationProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateIdeaAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<IdeaDetailModel>.Failure(
                    WorkflowFailureReason.ValidationError,
                    new[] { "Idea title is required." }))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.CreateIdea(
            Guid.NewGuid(),
            new IdeaWriteRequestModel
            {
                Title = string.Empty,
                Description = "desc",
                Priority = "Medium"
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(validation.Errors.TryGetValue("workflow", out var errors));
        Assert.Contains("Idea title is required.", errors);
    }

    [Fact]
    public async Task IdeasCreate_WhenUnauthorized_ReturnsUnauthorizedProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateIdeaAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.Unauthorized))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.CreateIdea(
            Guid.NewGuid(),
            new IdeaWriteRequestModel
            {
                Title = "Idea",
                Description = "desc",
                Priority = "Medium"
            },
            CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task IdeasCreate_WhenBoardNotFound_ReturnsNotFoundProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateIdeaAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.BoardNotFound))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.CreateIdea(
            Guid.NewGuid(),
            new IdeaWriteRequestModel
            {
                Title = "Idea",
                Description = "desc",
                Priority = "Medium"
            },
            CancellationToken.None);

        AssertProblem(result, StatusCodes.Status404NotFound, "Board not found.");
    }

    [Fact]
    public async Task IdeasToggleUpvote_WhenUnauthorized_ReturnsUnauthorizedProblem()
    {
        var service = new StubWorkflowManagementService
        {
            ToggleIdeaUpvoteAsyncHandler = (_, _, _) => Task.FromResult(
                WorkflowResult<UpvoteToggleResultModel>.Failure(WorkflowFailureReason.Unauthorized))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.ToggleUpvote(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task IdeasToggleUpvote_WhenIdeaNotFound_ReturnsNotFoundProblem()
    {
        var service = new StubWorkflowManagementService
        {
            ToggleIdeaUpvoteAsyncHandler = (_, _, _) => Task.FromResult(
                WorkflowResult<UpvoteToggleResultModel>.Failure(WorkflowFailureReason.IdeaNotFound))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.ToggleUpvote(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status404NotFound, "Idea not found.");
    }

    [Fact]
    public async Task IdeasCreateComment_WhenValidationError_ReturnsValidationProblem()
    {
        var service = new StubWorkflowManagementService
        {
            CreateCommentAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<CommentModel>.Failure(
                    WorkflowFailureReason.ValidationError,
                    new[] { "Comment body is required." }))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.CreateComment(
            Guid.NewGuid(),
            new CommentWriteRequestModel { Body = string.Empty },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(validation.Errors.TryGetValue("workflow", out var errors));
        Assert.Contains("Comment body is required.", errors);
    }

    [Fact]
    public async Task IdeasDeleteComment_WhenCommentNotFound_ReturnsNotFoundProblem()
    {
        var service = new StubWorkflowManagementService
        {
            DeleteCommentAsyncHandler = (_, _, _) => Task.FromResult(
                WorkflowResult.Failure(WorkflowFailureReason.CommentNotFound))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.DeleteComment(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status404NotFound, "Comment not found.");
    }

    [Fact]
    public async Task IdeasListTagSuggestions_WhenSucceeded_ReturnsOk()
    {
        var suggestions = new[] { "Security", "Service" };
        var organizationId = Guid.NewGuid();

        var service = new StubWorkflowManagementService
        {
            ListTagSuggestionsAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<IReadOnlyList<string>>.Success(suggestions))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), organizationId);

        var result = await controller.ListTagSuggestions(
            organizationId,
            new TagAutocompleteQueryModel { Search = "Se" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(suggestions, ok.Value);
    }

    [Fact]
    public async Task IdeasListTagSuggestions_WhenValidationError_ReturnsValidationProblem()
    {
        var service = new StubWorkflowManagementService
        {
            ListTagSuggestionsAsyncHandler = (_, _, _, _) => Task.FromResult(
                WorkflowResult<IReadOnlyList<string>>.Failure(
                    WorkflowFailureReason.ValidationError,
                    new[] { "Tag autocomplete requires at least 2 characters." }))
        };

        var controller = CreateIdeasController(service, Guid.NewGuid(), UserRole.User.ToString(), Guid.NewGuid());

        var result = await controller.ListTagSuggestions(
            Guid.NewGuid(),
            new TagAutocompleteQueryModel { Search = "A" },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var validation = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(validation.Errors.TryGetValue("workflow", out var errors));
        Assert.Contains("Tag autocomplete requires at least 2 characters.", errors);
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

    private static IdeasController CreateIdeasController(
        IWorkflowManagementService service,
        Guid userId,
        string role,
        Guid? organizationId)
    {
        var controller = new IdeasController(service)
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

        public Func<WorkflowActorContext, Guid, IdeaListQueryModel, CancellationToken, Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>>>? ListIdeasAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult<IdeaDetailModel>>>? GetIdeaDetailAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, IdeaWriteRequestModel, CancellationToken, Task<WorkflowResult<IdeaDetailModel>>>? CreateIdeaAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, IdeaWriteRequestModel, CancellationToken, Task<WorkflowResult<IdeaDetailModel>>>? UpdateIdeaAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, MoveIdeaStatusRequestModel, CancellationToken, Task<WorkflowResult>>? MoveIdeaStatusAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CommentListQueryModel, CancellationToken, Task<WorkflowResult<PagedResultModel<CommentModel>>>>? ListCommentsAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CommentWriteRequestModel, CancellationToken, Task<WorkflowResult<CommentModel>>>? CreateCommentAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CommentWriteRequestModel, CancellationToken, Task<WorkflowResult<CommentModel>>>? UpdateCommentAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult>>? DeleteCommentAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, CancellationToken, Task<WorkflowResult<UpvoteToggleResultModel>>>? ToggleIdeaUpvoteAsyncHandler { get; init; }

        public Func<WorkflowActorContext, Guid, TagAutocompleteQueryModel, CancellationToken, Task<WorkflowResult<IReadOnlyList<string>>>>? ListTagSuggestionsAsyncHandler { get; init; }

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

        public Task<WorkflowResult<PagedResultModel<IdeaListItemModel>>> ListIdeasAsync(
            WorkflowActorContext actor,
            Guid boardId,
            IdeaListQueryModel query,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ListIdeasAsyncHandler is null
                ? Task.FromResult(WorkflowResult<PagedResultModel<IdeaListItemModel>>.Failure(WorkflowFailureReason.Forbidden))
                : ListIdeasAsyncHandler(actor, boardId, query, cancellationToken);
        }

        public Task<WorkflowResult<IdeaDetailModel>> GetIdeaDetailAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return GetIdeaDetailAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : GetIdeaDetailAsyncHandler(actor, ideaId, cancellationToken);
        }

        public Task<WorkflowResult<IdeaDetailModel>> CreateIdeaAsync(
            WorkflowActorContext actor,
            Guid boardId,
            IdeaWriteRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return CreateIdeaAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : CreateIdeaAsyncHandler(actor, boardId, request, cancellationToken);
        }

        public Task<WorkflowResult<IdeaDetailModel>> UpdateIdeaAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            IdeaWriteRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return UpdateIdeaAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IdeaDetailModel>.Failure(WorkflowFailureReason.Forbidden))
                : UpdateIdeaAsyncHandler(actor, ideaId, request, cancellationToken);
        }

        public Task<WorkflowResult> MoveIdeaStatusAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            MoveIdeaStatusRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return MoveIdeaStatusAsyncHandler is null
                ? Task.FromResult(WorkflowResult.Failure(WorkflowFailureReason.Forbidden))
                : MoveIdeaStatusAsyncHandler(actor, ideaId, request, cancellationToken);
        }

        public Task<WorkflowResult> SoftDeleteIdeaAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return Task.FromResult(WorkflowResult.Failure(WorkflowFailureReason.Forbidden));
        }

        public Task<WorkflowResult<PagedResultModel<CommentModel>>> ListCommentsAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            CommentListQueryModel query,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ListCommentsAsyncHandler is null
                ? Task.FromResult(WorkflowResult<PagedResultModel<CommentModel>>.Failure(WorkflowFailureReason.Forbidden))
                : ListCommentsAsyncHandler(actor, ideaId, query, cancellationToken);
        }

        public Task<WorkflowResult<CommentModel>> CreateCommentAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            CommentWriteRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return CreateCommentAsyncHandler is null
                ? Task.FromResult(WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.Forbidden))
                : CreateCommentAsyncHandler(actor, ideaId, request, cancellationToken);
        }

        public Task<WorkflowResult<CommentModel>> UpdateCommentAsync(
            WorkflowActorContext actor,
            Guid commentId,
            CommentWriteRequestModel request,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return UpdateCommentAsyncHandler is null
                ? Task.FromResult(WorkflowResult<CommentModel>.Failure(WorkflowFailureReason.Forbidden))
                : UpdateCommentAsyncHandler(actor, commentId, request, cancellationToken);
        }

        public Task<WorkflowResult> DeleteCommentAsync(
            WorkflowActorContext actor,
            Guid commentId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return DeleteCommentAsyncHandler is null
                ? Task.FromResult(WorkflowResult.Failure(WorkflowFailureReason.Forbidden))
                : DeleteCommentAsyncHandler(actor, commentId, cancellationToken);
        }

        public Task<WorkflowResult<UpvoteToggleResultModel>> ToggleIdeaUpvoteAsync(
            WorkflowActorContext actor,
            Guid ideaId,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ToggleIdeaUpvoteAsyncHandler is null
                ? Task.FromResult(WorkflowResult<UpvoteToggleResultModel>.Failure(WorkflowFailureReason.Forbidden))
                : ToggleIdeaUpvoteAsyncHandler(actor, ideaId, cancellationToken);
        }

        public Task<WorkflowResult<IReadOnlyList<string>>> ListTagSuggestionsAsync(
            WorkflowActorContext actor,
            Guid organizationId,
            TagAutocompleteQueryModel query,
            CancellationToken cancellationToken)
        {
            LastActor = actor;
            return ListTagSuggestionsAsyncHandler is null
                ? Task.FromResult(WorkflowResult<IReadOnlyList<string>>.Failure(WorkflowFailureReason.Forbidden))
                : ListTagSuggestionsAsyncHandler(actor, organizationId, query, cancellationToken);
        }
    }
}

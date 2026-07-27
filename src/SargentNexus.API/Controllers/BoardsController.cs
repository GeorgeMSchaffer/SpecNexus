using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Workflow;

namespace SargentNexus.API.Controllers;

public sealed class BoardsController : ApiControllerBase
{
    private readonly IWorkflowManagementService _workflowService;

    public BoardsController(IWorkflowManagementService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/boards")]
    [ProducesResponseType(typeof(IReadOnlyList<BoardSummaryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListBoardsAsync(GetWorkflowActorContext(), organizationId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpGet("/api/v1/boards/{boardId:guid}")]
    [ProducesResponseType(typeof(BoardDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detail(Guid boardId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetBoardDetailAsync(GetWorkflowActorContext(), boardId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/boards")]
    [ProducesResponseType(typeof(BoardDetailModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid organizationId,
        [FromBody] CreateBoardRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateBoardAsync(GetWorkflowActorContext(), organizationId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Created($"/api/v1/boards/{result.Response!.BoardId}", result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPut("/api/v1/boards/{boardId:guid}")]
    [ProducesResponseType(typeof(BoardDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid boardId,
        [FromBody] UpdateBoardRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.UpdateBoardAsync(GetWorkflowActorContext(), boardId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/boards/{boardId:guid}/swimlanes/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        Guid boardId,
        [FromBody] ReorderSwimlanesRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.ReorderSwimlanesAsync(GetWorkflowActorContext(), boardId, request, cancellationToken);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    private IActionResult ToProblem(WorkflowFailureReason reason, IReadOnlyList<string> errors)
    {
        return reason switch
        {
            WorkflowFailureReason.Unauthorized => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required."),
            WorkflowFailureReason.Forbidden => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden.",
                detail: "The authenticated user is not allowed to perform this action."),
            WorkflowFailureReason.OrganizationNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Organization not found.",
                detail: "The organization could not be found."),
            WorkflowFailureReason.BoardNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Board not found.",
                detail: "The board could not be found."),
            WorkflowFailureReason.ValidationError => BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["workflow"] = errors.Count == 0 ? new[] { "Validation failed." } : errors.ToArray()
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            }),
            _ => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Unable to process workflow request.")
        };
    }
}

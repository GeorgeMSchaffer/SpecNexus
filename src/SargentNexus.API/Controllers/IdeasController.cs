using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Workflow;

namespace SargentNexus.API.Controllers;

public sealed class IdeasController : ApiControllerBase
{
    private readonly IWorkflowManagementService _workflowService;

    public IdeasController(IWorkflowManagementService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/tags")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListTagSuggestions(
        Guid organizationId,
        [FromQuery] TagAutocompleteQueryModel query,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListTagSuggestionsAsync(GetWorkflowActorContext(), organizationId, query, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpGet("/api/v1/boards/{boardId:guid}/ideas")]
    [ProducesResponseType(typeof(PagedResultModel<IdeaListItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListIdeas(
        Guid boardId,
        [FromQuery] IdeaListQueryModel query,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListIdeasAsync(GetWorkflowActorContext(), boardId, query, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/boards/{boardId:guid}/ideas")]
    [ProducesResponseType(typeof(IdeaDetailModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateIdea(
        Guid boardId,
        [FromBody] IdeaWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateIdeaAsync(GetWorkflowActorContext(), boardId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Created($"/api/v1/ideas/{result.Response!.IdeaId}", result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpGet("/api/v1/ideas/{ideaId:guid}")]
    [ProducesResponseType(typeof(IdeaDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detail(Guid ideaId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetIdeaDetailAsync(GetWorkflowActorContext(), ideaId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPut("/api/v1/ideas/{ideaId:guid}")]
    [ProducesResponseType(typeof(IdeaDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid ideaId,
        [FromBody] IdeaWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.UpdateIdeaAsync(GetWorkflowActorContext(), ideaId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/ideas/{ideaId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveStatus(
        Guid ideaId,
        [FromBody] MoveIdeaStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.MoveIdeaStatusAsync(GetWorkflowActorContext(), ideaId, request, cancellationToken);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpGet("/api/v1/ideas/{ideaId:guid}/comments")]
    [ProducesResponseType(typeof(PagedResultModel<CommentModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListComments(
        Guid ideaId,
        [FromQuery] CommentListQueryModel query,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListCommentsAsync(GetWorkflowActorContext(), ideaId, query, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/ideas/{ideaId:guid}/comments")]
    [ProducesResponseType(typeof(CommentModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment(
        Guid ideaId,
        [FromBody] CommentWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateCommentAsync(GetWorkflowActorContext(), ideaId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Created($"/api/v1/comments/{result.Response!.CommentId}", result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPut("/api/v1/comments/{commentId:guid}")]
    [ProducesResponseType(typeof(CommentModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(
        Guid commentId,
        [FromBody] CommentWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.UpdateCommentAsync(GetWorkflowActorContext(), commentId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpDelete("/api/v1/comments/{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.DeleteCommentAsync(GetWorkflowActorContext(), commentId, cancellationToken);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpDelete("/api/v1/ideas/{ideaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeleteIdea(Guid ideaId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.SoftDeleteIdeaAsync(GetWorkflowActorContext(), ideaId, cancellationToken);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/ideas/{ideaId:guid}/upvote/toggle")]
    [ProducesResponseType(typeof(UpvoteToggleResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleUpvote(Guid ideaId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.ToggleIdeaUpvoteAsync(GetWorkflowActorContext(), ideaId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
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
            WorkflowFailureReason.IdeaNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Idea not found.",
                detail: "The idea could not be found."),
            WorkflowFailureReason.CommentNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Comment not found.",
                detail: "The comment could not be found."),
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

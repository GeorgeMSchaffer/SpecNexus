using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Workflow;

namespace SargentNexus.API.Controllers;

public sealed class StatusesController : ApiControllerBase
{
    private readonly IWorkflowManagementService _workflowService;

    public StatusesController(IWorkflowManagementService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<StatusSummaryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListStatusesAsync(GetWorkflowActorContext(), organizationId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/statuses")]
    [ProducesResponseType(typeof(StatusSummaryModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid organizationId,
        [FromBody] CreateStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateStatusAsync(GetWorkflowActorContext(), organizationId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Created($"/api/v1/statuses/{result.Response!.StatusId}", result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPut("/api/v1/statuses/{statusId:guid}")]
    [ProducesResponseType(typeof(StatusSummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid statusId,
        [FromBody] UpdateStatusRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.UpdateStatusAsync(GetWorkflowActorContext(), statusId, request, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpDelete("/api/v1/statuses/{statusId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid statusId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.SoftDeleteStatusAsync(GetWorkflowActorContext(), statusId, cancellationToken);

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
            WorkflowFailureReason.StatusNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Status not found.",
                detail: "The status could not be found."),
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

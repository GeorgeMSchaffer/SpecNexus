using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Workflow;

namespace SargentNexus.API.Controllers;

public sealed class BusinessImpactsController : ApiControllerBase
{
    private readonly IWorkflowManagementService _workflowService;

    public BusinessImpactsController(IWorkflowManagementService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/business-impacts")]
    [ProducesResponseType(typeof(IReadOnlyList<BusinessImpactSummaryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.ListBusinessImpactsAsync(GetWorkflowActorContext(), organizationId, cancellationToken);

        return result.Succeeded
            ? Ok(result.Response)
            : ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/business-impacts")]
    [ProducesResponseType(typeof(BusinessImpactSummaryModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid organizationId,
        [FromBody] BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateBusinessImpactAsync(
            GetWorkflowActorContext(),
            organizationId,
            request,
            cancellationToken);

        return result.Succeeded
            ? Created($"/api/v1/business-impacts/{result.Response!.BusinessImpactId}", result.Response)
            : ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPut("/api/v1/business-impacts/{businessImpactId:guid}")]
    [ProducesResponseType(typeof(BusinessImpactSummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid businessImpactId,
        [FromBody] BusinessImpactWriteRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.UpdateBusinessImpactAsync(
            GetWorkflowActorContext(),
            businessImpactId,
            request,
            cancellationToken);

        return result.Succeeded
            ? Ok(result.Response)
            : ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/business-impacts/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        Guid organizationId,
        [FromBody] ReorderBusinessImpactsRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _workflowService.ReorderBusinessImpactsAsync(
            GetWorkflowActorContext(),
            organizationId,
            request,
            cancellationToken);

        return result.Succeeded
            ? NoContent()
            : ToProblem(result.FailureReason!.Value, result.Errors);
    }

    [HttpDelete("/api/v1/business-impacts/{businessImpactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid businessImpactId, CancellationToken cancellationToken)
    {
        var result = await _workflowService.SoftDeleteBusinessImpactAsync(GetWorkflowActorContext(), businessImpactId, cancellationToken);

        return result.Succeeded
            ? NoContent()
            : ToProblem(result.FailureReason!.Value, result.Errors);
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
            WorkflowFailureReason.BusinessImpactNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Business Impact not found.",
                detail: "The Business Impact could not be found."),
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
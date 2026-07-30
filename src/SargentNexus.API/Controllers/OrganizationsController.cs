using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Administration;

namespace SargentNexus.API.Controllers;

public sealed record InviteCodeResponseModel(string InviteCode)
{
    public InviteCodeResponseModel() : this(string.Empty) { }
    public string InviteCode { get; init; } = InviteCode;
}

public sealed class OrganizationsController : ApiControllerBase
{
    private readonly IOrganizationUserAdministrationService _service;

    public OrganizationsController(IOrganizationUserAdministrationService service)
    {
        _service = service;
    }

    [HttpGet("/api/v1/organizations")]
    [ProducesResponseType(typeof(PagedResultModel<OrganizationListItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListOrganizations([FromQuery] OrganizationListQueryModel request, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.ListOrganizationsAsync(actorUserId.Value, request, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    [HttpPost("/api/v1/organizations")]
    [ProducesResponseType(typeof(OrganizationCreateResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationUpsertRequestModel request, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.CreateOrganizationAsync(actorUserId.Value, request, cancellationToken);

        return ToActionResult(
            result,
            onSuccess: model => Created($"/api/v1/organizations/{model.OrganizationId}", model));
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}")]
    [ProducesResponseType(typeof(OrganizationDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization(Guid organizationId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.GetOrganizationAsync(actorUserId.Value, organizationId, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    [HttpPut("/api/v1/organizations/{organizationId:guid}")]
    [ProducesResponseType(typeof(OrganizationDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrganization(
        Guid organizationId,
        [FromBody] OrganizationUpsertRequestModel request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.UpdateOrganizationAsync(actorUserId.Value, organizationId, request, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/invite-code/regenerate")]
    [ProducesResponseType(typeof(InviteCodeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateInviteCode(Guid organizationId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.RegenerateInviteCodeAsync(actorUserId.Value, organizationId, cancellationToken);

        return ToActionResult(result, onSuccess: code => Ok(new InviteCodeResponseModel { InviteCode = code }));
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/archive")]    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveOrganization(Guid organizationId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.ArchiveOrganizationAsync(actorUserId.Value, organizationId, cancellationToken);

        return ToActionResult(result, onSuccess: _ => NoContent());
    }

    private IActionResult ToActionResult<T>(AdministrationResult<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.Succeeded)
        {
            return onSuccess(result.Value!);
        }

        return ToFailureResult(result.FailureReason, result.Errors);
    }

    private IActionResult ToActionResult(AdministrationResult result, Func<object?, IActionResult> onSuccess)
    {
        if (result.Succeeded)
        {
            return onSuccess(null);
        }

        return ToFailureResult(result.FailureReason, result.Errors);
    }

    private IActionResult ToFailureResult(AdministrationFailureReason? reason, IReadOnlyDictionary<string, string[]> errors)
    {
        return reason switch
        {
            AdministrationFailureReason.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found.",
                detail: "The requested resource could not be found."),
            AdministrationFailureReason.Forbidden => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden.",
                detail: "The authenticated user is not allowed to perform this action."),
            AdministrationFailureReason.ValidationFailed => BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>(errors))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            }),
            _ => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unable to complete request.")
        };
    }
}

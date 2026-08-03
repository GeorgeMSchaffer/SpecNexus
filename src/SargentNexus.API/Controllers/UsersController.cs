using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Administration;
using System.Text;

namespace SargentNexus.API.Controllers;

public sealed class UsersController : ApiControllerBase
{
    private readonly IOrganizationUserAdministrationService _service;

    public UsersController(IOrganizationUserAdministrationService service)
    {
        _service = service;
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/users")]
    [ProducesResponseType(typeof(PagedResultModel<UserSummaryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListUsers(
        Guid organizationId,
        [FromQuery] OrganizationUsersListQueryModel request,
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

        var result = await _service.ListUsersAsync(actorUserId.Value, organizationId, request, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/users")]
    [ProducesResponseType(typeof(UserCreateResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        Guid organizationId,
        [FromBody] UserCreateRequestModel request,
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

        var result = await _service.CreateUserAsync(actorUserId.Value, organizationId, request, cancellationToken);

        return ToActionResult(
            result,
            onSuccess: model => Created($"/api/v1/users/{model.UserId}", model));
    }

    [HttpGet("/api/v1/organizations/{organizationId:guid}/users/import-template")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadImportTemplate(Guid organizationId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.GetUserImportTemplateAsync(actorUserId.Value, organizationId, cancellationToken);
        return ToActionResult(
            result,
            onSuccess: csv => File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", "user-import-template.csv"));
    }

    [HttpPost("/api/v1/organizations/{organizationId:guid}/users/import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserImportResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportUsers(
        Guid organizationId,
        [FromForm] IFormFile? csvFile,
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

        if (csvFile is null)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["csvFile"] = new[] { "CSV file is required." }
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        if (csvFile.Length <= 0 || csvFile.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["csvFile"] = new[] { "CSV file must be between 1 byte and 5 MB." }
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        byte[] bytes;
        await using (var stream = csvFile.OpenReadStream())
        {
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            bytes = memory.ToArray();
        }

        var result = await _service.ImportUsersCsvAsync(actorUserId.Value, organizationId, bytes, cancellationToken);
        return ToActionResult(
            result,
            onSuccess: model => Created($"/api/v1/organizations/{organizationId}/users/import", model));
    }

    [HttpGet("/api/v1/users/{userId:guid}")]
    [ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _service.GetUserAsync(actorUserId.Value, userId, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    [HttpPut("/api/v1/users/{userId:guid}")]
    [ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(
        Guid userId,
        [FromBody] UserUpdateRequestModel request,
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

        var result = await _service.UpdateUserAsync(actorUserId.Value, userId, request, cancellationToken);

        return ToActionResult(result, onSuccess: model => Ok(model));
    }

    private IActionResult ToActionResult<T>(AdministrationResult<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.Succeeded)
        {
            return onSuccess(result.Value!);
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

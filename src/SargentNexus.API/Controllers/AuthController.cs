using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SargentNexus.Application.Auth;

namespace SargentNexus.API.Controllers;

[AllowAnonymous]
public sealed class AuthController : ApiControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IAuthAccountService _authAccountService;

    public AuthController(ILoginService loginService, IAuthAccountService authAccountService)
    {
        _loginService = loginService;
        _authAccountService = authAccountService;
    }

    [HttpPost("/api/v1/auth/login")]
    [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request, CancellationToken cancellationToken)
    {
        var result = await _loginService.LoginAsync(request, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return result.FailureReason switch
        {
            LoginFailureReason.LockedOut => Problem(
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "User account is locked out.",
                detail: "Too many failed login attempts have temporarily locked this account."),
            LoginFailureReason.InactiveUser => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "User account is inactive.",
                detail: "The user account is inactive and cannot authenticate."),
            _ => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials.",
                detail: "The supplied credentials are invalid.")
        };
    }

    [HttpGet("/api/v1/auth/me")]
    [ProducesResponseType(typeof(AuthenticatedUserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var currentUser = await _authAccountService.GetCurrentUserAsync(userId.Value, cancellationToken);

        if (currentUser is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "The authenticated user could not be resolved.");
        }

        return Ok(currentUser);
    }

    [HttpPost("/api/v1/auth/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _authAccountService.ChangePasswordAsync(userId.Value, request, cancellationToken);

        if (result.Succeeded)
        {
            return NoContent();
        }

        if (result.FailureReason == ChangePasswordFailureReason.InvalidCurrentPassword)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid current password.",
                detail: "The supplied current password is invalid.");
        }

        if (result.FailureReason == ChangePasswordFailureReason.InvalidPasswordPolicy)
        {
            var validationProblem = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.NewPassword)] = result.Errors.ToArray()
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };

            return BadRequest(validationProblem);
        }

        return Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Authentication required.",
            detail: "The authenticated user could not be resolved.");
    }

    [HttpPost("/api/v1/users/{userId:guid}/temporary-password")]
    [ProducesResponseType(typeof(TemporaryPasswordResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueTemporaryPassword(Guid userId, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();

        if (!actorUserId.HasValue)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid bearer token is required.");
        }

        var result = await _authAccountService.IssueTemporaryPasswordAsync(actorUserId.Value, userId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        return result.FailureReason switch
        {
            TemporaryPasswordFailureReason.Forbidden => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden.",
                detail: "The authenticated user is not allowed to issue a temporary password for this account."),
            TemporaryPasswordFailureReason.UserNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "User not found.",
                detail: "The target user could not be found."),
            _ => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unable to issue temporary password.")
        };
    }
}
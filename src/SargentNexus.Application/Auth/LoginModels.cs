using System.ComponentModel.DataAnnotations;

namespace SargentNexus.Application.Auth;

public sealed class LoginRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }
}

public sealed class LoginUserModel
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string Role { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class LoginOrganizationOptionModel
{
    public Guid OrganizationId { get; init; }

    public string OrganizationName { get; init; } = string.Empty;
}

public sealed class LoginResponseModel
{
    public string? AccessToken { get; init; }

    public int? ExpiresInSeconds { get; init; }

    public bool? RequiresPasswordChange { get; init; }

    public LoginUserModel? User { get; init; }

    public bool? RequiresOrganizationSelection { get; init; }

    public IReadOnlyList<LoginOrganizationOptionModel>? Organizations { get; init; }
}

public sealed class ChangePasswordRequestModel
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

public enum LoginFailureReason
{
    InvalidCredentials = 1,
    InactiveUser = 2,
    LockedOut = 3
}

public sealed class LoginResult
{
    private LoginResult(bool succeeded, LoginResponseModel? response, LoginFailureReason? failureReason)
    {
        Succeeded = succeeded;
        Response = response;
        FailureReason = failureReason;
    }

    public bool Succeeded { get; }

    public LoginResponseModel? Response { get; }

    public LoginFailureReason? FailureReason { get; }

    public static LoginResult Success(LoginResponseModel response)
    {
        return new LoginResult(true, response, null);
    }

    public static LoginResult Failure(LoginFailureReason reason)
    {
        return new LoginResult(false, null, reason);
    }
}

public sealed class PasswordPolicyValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

public enum ChangePasswordFailureReason
{
    InvalidCurrentPassword = 1,
    InvalidPasswordPolicy = 2,
    UserNotFound = 3
}

public sealed class ChangePasswordResult
{
    private ChangePasswordResult(bool succeeded, ChangePasswordFailureReason? failureReason, IReadOnlyList<string>? errors)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool Succeeded { get; }

    public ChangePasswordFailureReason? FailureReason { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ChangePasswordResult Success()
    {
        return new ChangePasswordResult(true, null, null);
    }

    public static ChangePasswordResult Failure(ChangePasswordFailureReason reason, IReadOnlyList<string>? errors = null)
    {
        return new ChangePasswordResult(false, reason, errors);
    }
}

public sealed class AuthenticatedUserModel
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string Role { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class TemporaryPasswordResponseModel
{
    public string TemporaryPassword { get; init; } = string.Empty;

    public bool MustChangePassword { get; init; }
}

public enum TemporaryPasswordFailureReason
{
    UserNotFound = 1,
    Forbidden = 2
}

public sealed class TemporaryPasswordResult
{
    private TemporaryPasswordResult(bool succeeded, TemporaryPasswordResponseModel? response, TemporaryPasswordFailureReason? failureReason)
    {
        Succeeded = succeeded;
        Response = response;
        FailureReason = failureReason;
    }

    public bool Succeeded { get; }

    public TemporaryPasswordResponseModel? Response { get; }

    public TemporaryPasswordFailureReason? FailureReason { get; }

    public static TemporaryPasswordResult Success(TemporaryPasswordResponseModel response)
    {
        return new TemporaryPasswordResult(true, response, null);
    }

    public static TemporaryPasswordResult Failure(TemporaryPasswordFailureReason reason)
    {
        return new TemporaryPasswordResult(false, null, reason);
    }
}
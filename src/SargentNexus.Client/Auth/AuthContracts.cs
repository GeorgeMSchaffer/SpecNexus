namespace SargentNexus.Client.Auth;

public sealed class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class LoginUserDto
{
    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public string? AccessToken { get; set; }

    public int? ExpiresInSeconds { get; set; }

    public bool? RequiresPasswordChange { get; set; }

    public LoginUserDto? User { get; set; }
}

public sealed class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public sealed class UpdateProfileRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

public sealed class SelfRegistrationRequestDto
{
    public string InviteCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class ProblemDetailsDto
{
    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public string? Instance { get; set; }
}

public sealed class ValidationProblemDetailsDto : ProblemDetailsDto
{
    public Dictionary<string, string[]>? Errors { get; set; }
}

public sealed class AuthenticatedUserDto
{
    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class AuthApiException : Exception
{
    public AuthApiException(int statusCode, string title, string detail)
        : base(title)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public string Detail { get; }
}

public sealed class AuthValidationException : AuthApiException
{
    public AuthValidationException(string title, string detail, Dictionary<string, string[]> errors)
        : base(400, title, detail)
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}

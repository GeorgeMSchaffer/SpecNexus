using System.Security.Claims;

namespace SargentNexus.Client.Auth;

public interface IAuthSessionService
{
    event Action? StateChanged;

    bool IsAuthenticated { get; }

    bool RequiresPasswordChange { get; }

    string? AccessToken { get; }

    LoginUserDto? User { get; }

    string? LastAuthErrorTitle { get; }

    string? LastAuthErrorDetail { get; }

    ClaimsPrincipal CreatePrincipal();

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<LoginResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(string firstName, string lastName, string email, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    Task LogoutAsync();
}


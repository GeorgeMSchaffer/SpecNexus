using Microsoft.JSInterop;
using System.Security.Claims;

namespace SargentNexus.Client.Auth;

public sealed class AuthSessionService : IAuthSessionService
{
    private const string StorageAccessTokenKey = "sn.auth.accessToken";
    private const string StorageAccessTokenExpiresAtKey = "sn.auth.accessTokenExpiresAtUtc";
    private const string StorageRequiresPasswordChangeKey = "sn.auth.requiresPasswordChange";
    private const string StorageUserEmailKey = "sn.auth.userEmail";
    private const string StorageUserRoleKey = "sn.auth.userRole";
    private const string StorageUserIdKey = "sn.auth.userId";
    private const string StorageOrganizationIdKey = "sn.auth.organizationId";
    private const string StorageFirstNameKey = "sn.auth.firstName";
    private const string StorageLastNameKey = "sn.auth.lastName";
    private const string StorageStatusKey = "sn.auth.status";

    private readonly AuthApiClient _authApiClient;
    private readonly IJSRuntime _jsRuntime;

    private string? _accessToken;
    private DateTimeOffset? _accessTokenExpiresAtUtc;
    private bool _requiresPasswordChange;
    private LoginUserDto? _user;
    private bool _initialized;

    public AuthSessionService(AuthApiClient authApiClient, IJSRuntime jsRuntime)
    {
        _authApiClient = authApiClient;
        _jsRuntime = jsRuntime;
    }

    public event Action? StateChanged;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken) && _user is not null && !IsAccessTokenExpired();

    public bool RequiresPasswordChange => _requiresPasswordChange;

    public string? AccessToken => _accessToken;

    public LoginUserDto? User => _user;

    public string? LastAuthErrorTitle { get; private set; }

    public string? LastAuthErrorDetail { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized && IsAuthenticated)
        {
            return;
        }

        _initialized = true;

        _accessToken = await ReadStorageAsync(StorageAccessTokenKey);
        _accessTokenExpiresAtUtc = await ReadExpiresAtUtcAsync(StorageAccessTokenExpiresAtKey);
        _requiresPasswordChange = bool.TryParse(await ReadStorageAsync(StorageRequiresPasswordChangeKey), out var parsed) && parsed;

        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            await ClearSessionAsync();
            NotifyStateChanged();
            return;
        }

        if (IsAccessTokenExpired())
        {
            await ClearSessionAsync();
            NotifyStateChanged();
            return;
        }

        var me = await _authApiClient.GetCurrentUserAsync(_accessToken, cancellationToken);
        if (me is null)
        {
            await ClearSessionAsync();
            NotifyStateChanged();
            return;
        }

        _user = new LoginUserDto
        {
            UserId = me.UserId,
            OrganizationId = me.OrganizationId,
            Role = me.Role,
            FirstName = me.FirstName,
            LastName = me.LastName,
            Email = me.Email,
            Status = me.Status
        };

        NotifyStateChanged();
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        LastAuthErrorTitle = null;
        LastAuthErrorDetail = null;

        try
        {
            var response = await _authApiClient.LoginAsync(new LoginRequestDto
            {
                Email = email,
                Password = password
            }, cancellationToken);

            _accessToken = response.AccessToken;
            _accessTokenExpiresAtUtc = response.ExpiresInSeconds.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(response.ExpiresInSeconds.Value)
                : null;
            _requiresPasswordChange = response.RequiresPasswordChange == true;
            _user = response.User;

            await PersistSessionAsync();
            NotifyStateChanged();

            return response;
        }
        catch (AuthApiException ex)
        {
            LastAuthErrorTitle = ex.Title;
            LastAuthErrorDetail = ex.Detail;
            await ClearSessionAsync();
            NotifyStateChanged();
            throw;
        }
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            throw new InvalidOperationException("Cannot change password without an authenticated session.");
        }

        await _authApiClient.ChangePasswordAsync(_accessToken, new ChangePasswordRequestDto
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        }, cancellationToken);

        _requiresPasswordChange = false;
        await WriteStorageAsync(StorageRequiresPasswordChangeKey, "false");
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        await ClearSessionAsync();
        NotifyStateChanged();
    }

    public ClaimsPrincipal CreatePrincipal()
    {
        if (!IsAuthenticated || _user is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _user.UserId.ToString()),
            new(ClaimTypes.Email, _user.Email),
            new(ClaimTypes.Role, _user.Role)
        };

        if (!string.IsNullOrWhiteSpace(_user.FirstName) || !string.IsNullOrWhiteSpace(_user.LastName))
        {
            claims.Add(new Claim(ClaimTypes.Name, $"{_user.FirstName} {_user.LastName}".Trim()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "client-token"));
    }

    private async Task PersistSessionAsync()
    {
        await WriteStorageAsync(StorageAccessTokenKey, _accessToken);
        await WriteStorageAsync(StorageAccessTokenExpiresAtKey, _accessTokenExpiresAtUtc?.ToString("O"));
        await WriteStorageAsync(StorageRequiresPasswordChangeKey, _requiresPasswordChange.ToString().ToLowerInvariant());
        await WriteStorageAsync(StorageUserEmailKey, _user?.Email);
        await WriteStorageAsync(StorageUserRoleKey, _user?.Role);
        await WriteStorageAsync(StorageUserIdKey, _user?.UserId.ToString());
        await WriteStorageAsync(StorageOrganizationIdKey, _user?.OrganizationId?.ToString());
        await WriteStorageAsync(StorageFirstNameKey, _user?.FirstName);
        await WriteStorageAsync(StorageLastNameKey, _user?.LastName);
        await WriteStorageAsync(StorageStatusKey, _user?.Status);
    }

    private async Task ClearSessionAsync()
    {
        _accessToken = null;
        _accessTokenExpiresAtUtc = null;
        _user = null;
        _requiresPasswordChange = false;

        await WriteStorageAsync(StorageAccessTokenKey, null);
        await WriteStorageAsync(StorageAccessTokenExpiresAtKey, null);
        await WriteStorageAsync(StorageRequiresPasswordChangeKey, null);
        await WriteStorageAsync(StorageUserEmailKey, null);
        await WriteStorageAsync(StorageUserRoleKey, null);
        await WriteStorageAsync(StorageUserIdKey, null);
        await WriteStorageAsync(StorageOrganizationIdKey, null);
        await WriteStorageAsync(StorageFirstNameKey, null);
        await WriteStorageAsync(StorageLastNameKey, null);
        await WriteStorageAsync(StorageStatusKey, null);
    }

    private async Task<string?> ReadStorageAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    private async Task WriteStorageAsync(string key, string? value)
    {
        if (value is null)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            return;
        }

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    private async Task<DateTimeOffset?> ReadExpiresAtUtcAsync(string key)
    {
        var value = await ReadStorageAsync(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private bool IsAccessTokenExpired()
    {
        return _accessTokenExpiresAtUtc.HasValue && _accessTokenExpiresAtUtc.Value <= DateTimeOffset.UtcNow;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}

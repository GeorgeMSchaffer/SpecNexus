﻿using System.Security.Claims;

namespace SargentNexus.Client.Auth;

public sealed class AuthSessionService : IAuthSessionService, IDisposable
{
    private readonly AuthApiClient _authApiClient;
    private readonly AuthSessionStorage _sessionStorage;

    private string? _accessToken;
    private bool _requiresPasswordChange;
    private LoginUserDto? _user;
    private bool _initialized;

    public AuthSessionService(AuthApiClient authApiClient, AuthSessionStorage sessionStorage)
    {
        _authApiClient = authApiClient;
        _sessionStorage = sessionStorage;
        _sessionStorage.Invalidated += HandleSessionInvalidated;
    }

    public event Action? StateChanged;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken) && _user is not null;

    public bool RequiresPasswordChange => _requiresPasswordChange;

    public string? AccessToken => _accessToken;

    public LoginUserDto? User => _user;

    public string? LastAuthErrorTitle { get; private set; }

    public string? LastAuthErrorDetail { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized && !string.IsNullOrWhiteSpace(_accessToken) && _user is not null)
        {
            return;
        }

        _initialized = true;

        var storedAccessToken = await _sessionStorage.ReadAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(storedAccessToken))
        {
            await ClearSessionAsync();
            NotifyStateChanged();
            return;
        }

        var currentUser = await _authApiClient.GetCurrentUserAsync(storedAccessToken, cancellationToken);
        if (currentUser is null)
        {
            await ClearSessionAsync();
            NotifyStateChanged();
            return;
        }

        _accessToken = storedAccessToken;
        _requiresPasswordChange = await _sessionStorage.ReadRequiresPasswordChangeAsync();
        _user = new LoginUserDto
        {
            UserId = currentUser.UserId,
            OrganizationId = currentUser.OrganizationId,
            Role = currentUser.Role,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email,
            Status = currentUser.Status
        };

        await PersistSessionAsync();
        await PersistSessionAsync();
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
        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public async Task UpdateProfileAsync(string firstName, string lastName, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            throw new InvalidOperationException("Cannot update a profile without an authenticated session.");
        }

        var updatedUser = await _authApiClient.UpdateProfileAsync(_accessToken, new UpdateProfileRequestDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        }, cancellationToken);

        _user = new LoginUserDto
        {
            UserId = updatedUser.UserId,
            OrganizationId = updatedUser.OrganizationId,
            Role = updatedUser.Role,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Email = updatedUser.Email,
            Status = updatedUser.Status
        };
        await PersistSessionAsync();
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
        await _sessionStorage.PersistAsync(_accessToken, _requiresPasswordChange, _user);
    }

    private async Task ClearSessionAsync()
    {
        await _sessionStorage.ClearAsync();
    }

    private void HandleSessionInvalidated()
    {
        _accessToken = null;
        _user = null;
        _requiresPasswordChange = false;
        NotifyStateChanged();
    }

    public void Dispose()
    {
        _sessionStorage.Invalidated -= HandleSessionInvalidated;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}




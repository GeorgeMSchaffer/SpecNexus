using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SargentNexus.Client.Auth;

public sealed class ClientAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthSessionService _authSessionService;
    private readonly AuthenticationState _anonymous;

    public ClientAuthStateProvider(IAuthSessionService authSessionService)
    {
        _authSessionService = authSessionService;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        _authSessionService.StateChanged += OnSessionStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _authSessionService.InitializeAsync();

        if (!_authSessionService.IsAuthenticated)
        {
            return _anonymous;
        }

        return new AuthenticationState(_authSessionService.CreatePrincipal());
    }

    private void OnSessionStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Dispose()
    {
        _authSessionService.StateChanged -= OnSessionStateChanged;
    }
}

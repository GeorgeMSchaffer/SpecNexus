using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace SargentNexus.Client.Auth;

public sealed class AuthenticatedSessionHandler : DelegatingHandler
{
    private const string CurrentUserPath = "/api/v1/auth/me";

    private readonly AuthSessionStorage _sessionStorage;
    private readonly NavigationManager _navigationManager;

    public AuthenticatedSessionHandler(
        AuthSessionStorage sessionStorage,
        NavigationManager navigationManager)
    {
        _sessionStorage = sessionStorage;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var accessToken = request.Headers.Authorization?.Scheme == "Bearer"
            ? request.Headers.Authorization.Parameter
            : null;

        if (response.StatusCode != HttpStatusCode.Unauthorized ||
            string.IsNullOrWhiteSpace(accessToken) ||
            IsCurrentUserValidationRequest(request))
        {
            return response;
        }

        try
        {
            using var validationRequest = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/me");
            validationRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var validationResponse = await base.SendAsync(validationRequest, cancellationToken);
            if (validationResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _sessionStorage.ClearAsync();
                RedirectToLogin();
            }
        }
        catch (HttpRequestException)
        {
            // Preserve the original response when token validity cannot be confirmed.
        }

        return response;
    }

    private static bool IsCurrentUserValidationRequest(HttpRequestMessage request)
    {
        return request.Method == HttpMethod.Get &&
            string.Equals(request.RequestUri?.AbsolutePath, CurrentUserPath, StringComparison.OrdinalIgnoreCase);
    }

    private void RedirectToLogin()
    {
        var path = _navigationManager
            .ToBaseRelativePath(_navigationManager.Uri)
            .Split('?', '#')[0]
            .Trim('/');

        if (!string.Equals(path, "login", StringComparison.OrdinalIgnoreCase))
        {
            _navigationManager.NavigateTo("/login", replace: true);
        }
    }
}
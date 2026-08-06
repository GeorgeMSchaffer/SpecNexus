using Microsoft.JSInterop;

namespace SargentNexus.Client.Auth;

public sealed class AuthSessionStorage
{
    private const string StorageAccessTokenKey = "sn.auth.accessToken";
    private const string StorageRequiresPasswordChangeKey = "sn.auth.requiresPasswordChange";
    private const string StorageUserEmailKey = "sn.auth.userEmail";
    private const string StorageUserRoleKey = "sn.auth.userRole";
    private const string StorageUserIdKey = "sn.auth.userId";
    private const string StorageOrganizationIdKey = "sn.auth.organizationId";
    private const string StorageFirstNameKey = "sn.auth.firstName";
    private const string StorageLastNameKey = "sn.auth.lastName";
    private const string StorageStatusKey = "sn.auth.status";

    private static readonly string[] StorageKeys =
    {
        StorageAccessTokenKey,
        StorageRequiresPasswordChangeKey,
        StorageUserEmailKey,
        StorageUserRoleKey,
        StorageUserIdKey,
        StorageOrganizationIdKey,
        StorageFirstNameKey,
        StorageLastNameKey,
        StorageStatusKey
    };

    private readonly IJSRuntime _jsRuntime;

    public AuthSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? Invalidated;

    public Task<string?> ReadAccessTokenAsync()
    {
        return ReadAsync(StorageAccessTokenKey);
    }

    public async Task<bool> ReadRequiresPasswordChangeAsync()
    {
        return bool.TryParse(await ReadAsync(StorageRequiresPasswordChangeKey), out var parsed) && parsed;
    }

    public async Task PersistAsync(string? accessToken, bool requiresPasswordChange, LoginUserDto? user)
    {
        await WriteAsync(StorageAccessTokenKey, accessToken);
        await WriteAsync(StorageRequiresPasswordChangeKey, requiresPasswordChange.ToString().ToLowerInvariant());
        await WriteAsync(StorageUserEmailKey, user?.Email);
        await WriteAsync(StorageUserRoleKey, user?.Role);
        await WriteAsync(StorageUserIdKey, user?.UserId.ToString());
        await WriteAsync(StorageOrganizationIdKey, user?.OrganizationId?.ToString());
        await WriteAsync(StorageFirstNameKey, user?.FirstName);
        await WriteAsync(StorageLastNameKey, user?.LastName);
        await WriteAsync(StorageStatusKey, user?.Status);
    }

    public async Task ClearAsync()
    {
        foreach (var key in StorageKeys)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        Invalidated?.Invoke();
    }

    private async Task<string?> ReadAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    private async Task WriteAsync(string key, string? value)
    {
        if (value is null)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            return;
        }

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }
}
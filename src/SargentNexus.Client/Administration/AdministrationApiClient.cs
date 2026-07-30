using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SargentNexus.Client.Auth;

namespace SargentNexus.Client.Administration;

public sealed class AdministrationApiClient
{
    private readonly HttpClient _httpClient;

    public AdministrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResultDto<OrganizationListItemDto>> ListOrganizationsAsync(
        string accessToken,
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"isArchived={includeArchived.ToString().ToLowerInvariant()}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query.Add($"sortBy={Uri.EscapeDataString(sortBy.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(sortDirection))
        {
            query.Add($"sortDirection={Uri.EscapeDataString(sortDirection.Trim())}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations?{string.Join("&", query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<OrganizationListItemDto>>(cancellationToken: cancellationToken)
                ?? new PagedResultDto<OrganizationListItemDto>();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<OrganizationCreateResponseDto> CreateOrganizationAsync(
        string accessToken,
        OrganizationUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/organizations")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            return await response.Content.ReadFromJsonAsync<OrganizationCreateResponseDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return an organization payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<OrganizationDetailDto> GetOrganizationAsync(
        string accessToken,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations/{organizationId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrganizationDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return an organization detail payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<OrganizationDetailDto> UpdateOrganizationAsync(
        string accessToken,
        Guid organizationId,
        OrganizationUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/v1/organizations/{organizationId}")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrganizationDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return an organization detail payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task ArchiveOrganizationAsync(string accessToken, Guid organizationId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/organizations/{organizationId}/archive");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return;
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<PagedResultDto<UserListItemDto>> ListUsersAsync(
        string accessToken,
        Guid organizationId,
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations/{organizationId}/users?{string.Join("&", query)}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<UserListItemDto>>(cancellationToken: cancellationToken)
                ?? new PagedResultDto<UserListItemDto>();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<InviteCodeResponseDto> RegenerateInviteCodeAsync(string accessToken, Guid organizationId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/organizations/{organizationId}/invite-code/regenerate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InviteCodeResponseDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return an invite code payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<UserDetailDto> GetUserAsync(
        string accessToken,
        Guid userId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return a user payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<UserCreateResponseDto> CreateUserAsync(
        string accessToken,
        Guid organizationId,
        UserCreateRequestDto request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/v1/organizations/{organizationId}/users")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            return await response.Content.ReadFromJsonAsync<UserCreateResponseDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return a user payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<UserDetailDto> UpdateUserAsync(
        string accessToken,
        Guid userId,
        UserUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{userId}")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("The API did not return a user payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    private static async Task<AuthApiException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken: cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsDto>(cancellationToken: cancellationToken);
            if (validationProblem?.Errors is { Count: > 0 })
            {
                return new AuthValidationException(
                    validationProblem.Title ?? "Validation failed.",
                    validationProblem.Detail ?? "One or more validation errors occurred.",
                    validationProblem.Errors);
            }
        }

        return new AuthApiException(
            (int)response.StatusCode,
            problem?.Title ?? "Administration error",
            problem?.Detail ?? "An administration API error occurred.");
    }
}

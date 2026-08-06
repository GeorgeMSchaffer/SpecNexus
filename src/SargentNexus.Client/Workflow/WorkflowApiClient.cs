using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SargentNexus.Client.Auth;

namespace SargentNexus.Client.Workflow;

public sealed class WorkflowApiClient
{
    private readonly HttpClient _httpClient;

    public WorkflowApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<BoardSummaryDto>> ListBoardsAsync(string accessToken, Guid organizationId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations/{organizationId}/boards");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<BoardSummaryDto>>(cancellationToken: cancellationToken)
            ?? Array.Empty<BoardSummaryDto>();
    }

    public async Task<IReadOnlyList<StatusSummaryDto>> ListStatusesAsync(string accessToken, Guid organizationId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations/{organizationId}/statuses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<StatusSummaryDto>>(cancellationToken: cancellationToken)
            ?? Array.Empty<StatusSummaryDto>();
    }

    public async Task<IReadOnlyList<IdeaTypeSummaryDto>> ListIdeaTypesAsync(
        string accessToken,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/v1/organizations/{organizationId}/idea-types",
            accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<IdeaTypeSummaryDto>>(cancellationToken: cancellationToken)
                ?? Array.Empty<IdeaTypeSummaryDto>();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public Task<IdeaTypeSummaryDto> CreateIdeaTypeAsync(
        string accessToken,
        Guid organizationId,
        IdeaTypeWriteRequestDto request,
        CancellationToken cancellationToken) =>
        SendForPayloadAsync<IdeaTypeSummaryDto>(
            HttpMethod.Post,
            $"api/v1/organizations/{organizationId}/idea-types",
            accessToken,
            request,
            "Idea Type",
            cancellationToken);

    public Task<IdeaTypeSummaryDto> UpdateIdeaTypeAsync(
        string accessToken,
        Guid ideaTypeId,
        IdeaTypeWriteRequestDto request,
        CancellationToken cancellationToken) =>
        SendForPayloadAsync<IdeaTypeSummaryDto>(
            HttpMethod.Put,
            $"api/v1/idea-types/{ideaTypeId}",
            accessToken,
            request,
            "Idea Type",
            cancellationToken);

    public Task ReorderIdeaTypesAsync(
        string accessToken,
        Guid organizationId,
        ReorderIdeaTypesRequestDto request,
        CancellationToken cancellationToken) =>
        SendForNoContentAsync(
            HttpMethod.Post,
            $"api/v1/organizations/{organizationId}/idea-types/reorder",
            accessToken,
            request,
            cancellationToken);

    public Task DeleteIdeaTypeAsync(
        string accessToken,
        Guid ideaTypeId,
        CancellationToken cancellationToken) =>
        SendForNoContentAsync(
            HttpMethod.Delete,
            $"api/v1/idea-types/{ideaTypeId}",
            accessToken,
            content: null,
            cancellationToken);

    public async Task<IReadOnlyList<BusinessImpactSummaryDto>> ListBusinessImpactsAsync(
        string accessToken,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/v1/organizations/{organizationId}/business-impacts",
            accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<BusinessImpactSummaryDto>>(cancellationToken: cancellationToken)
                ?? Array.Empty<BusinessImpactSummaryDto>();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public Task<BusinessImpactSummaryDto> CreateBusinessImpactAsync(
        string accessToken,
        Guid organizationId,
        BusinessImpactWriteRequestDto request,
        CancellationToken cancellationToken) =>
        SendForPayloadAsync<BusinessImpactSummaryDto>(
            HttpMethod.Post,
            $"api/v1/organizations/{organizationId}/business-impacts",
            accessToken,
            request,
            "Business Impact",
            cancellationToken);

    public Task<BusinessImpactSummaryDto> UpdateBusinessImpactAsync(
        string accessToken,
        Guid businessImpactId,
        BusinessImpactWriteRequestDto request,
        CancellationToken cancellationToken) =>
        SendForPayloadAsync<BusinessImpactSummaryDto>(
            HttpMethod.Put,
            $"api/v1/business-impacts/{businessImpactId}",
            accessToken,
            request,
            "Business Impact",
            cancellationToken);

    public Task ReorderBusinessImpactsAsync(
        string accessToken,
        Guid organizationId,
        ReorderBusinessImpactsRequestDto request,
        CancellationToken cancellationToken) =>
        SendForNoContentAsync(
            HttpMethod.Post,
            $"api/v1/organizations/{organizationId}/business-impacts/reorder",
            accessToken,
            request,
            cancellationToken);

    public Task DeleteBusinessImpactAsync(
        string accessToken,
        Guid businessImpactId,
        CancellationToken cancellationToken) =>
        SendForNoContentAsync(
            HttpMethod.Delete,
            $"api/v1/business-impacts/{businessImpactId}",
            accessToken,
            content: null,
            cancellationToken);

    public async Task<PagedResultDto<IdeaListItemDto>> ListIdeasAsync(string accessToken, Guid boardId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/boards/{boardId}/ideas?page=1&pageSize=50");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResultDto<IdeaListItemDto>>(cancellationToken: cancellationToken)
            ?? new PagedResultDto<IdeaListItemDto>();
    }

    public async Task<BoardSummaryDto> CreateBoardAsync(string accessToken, Guid organizationId, CreateBoardRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/organizations/{organizationId}/boards")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BoardSummaryDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a board payload.");
    }

    public async Task<IdeaDetailDto> CreateIdeaAsync(string accessToken, Guid boardId, IdeaWriteRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/boards/{boardId}/ideas")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IdeaDetailDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return an idea payload.");
    }

    public async Task<IdeaDetailDto> GetIdeaDetailAsync(string accessToken, Guid ideaId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/ideas/{ideaId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IdeaDetailDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return an idea payload.");
    }

    public Task<IdeaDetailDto> UpdateIdeaAsync(
        string accessToken,
        Guid ideaId,
        IdeaWriteRequestDto request,
        CancellationToken cancellationToken) =>
        SendForPayloadAsync<IdeaDetailDto>(
            HttpMethod.Put,
            $"api/v1/ideas/{ideaId}",
            accessToken,
            request,
            "idea",
            cancellationToken);

    public Task DeleteIdeaAsync(string accessToken, Guid ideaId, CancellationToken cancellationToken) =>
        SendForNoContentAsync(
            HttpMethod.Delete,
            $"api/v1/ideas/{ideaId}",
            accessToken,
            content: null,
            cancellationToken);

    public async Task<IReadOnlyList<string>> ListTagSuggestionsAsync(
        string accessToken,
        Guid organizationId,
        string search,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/v1/organizations/{organizationId}/tags?search={Uri.EscapeDataString(search.Trim())}&limit=10",
            accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>(cancellationToken: cancellationToken)
                ?? Array.Empty<string>();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<CommentDto> CreateCommentAsync(string accessToken, Guid ideaId, CommentWriteRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/ideas/{ideaId}/comments")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CommentDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a comment payload.");
    }

    public async Task<BoardSummaryDto> GetBoardDetailAsync(string accessToken, Guid boardId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/boards/{boardId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BoardSummaryDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a board payload.");
    }

    public async Task<UpvoteToggleResultDto> ToggleUpvoteAsync(string accessToken, Guid ideaId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/ideas/{ideaId}/upvote/toggle");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UpvoteToggleResultDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return an upvote payload.");
    }

    public async Task MoveIdeaStatusAsync(string accessToken, Guid ideaId, Guid statusId, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/ideas/{ideaId}/status")
        {
            Content = JsonContent.Create(new MoveIdeaStatusRequestDto { StatusId = statusId })
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BoardSummaryDto> UpdateBoardAsync(string accessToken, Guid boardId, UpdateBoardRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/boards/{boardId}")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BoardSummaryDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a board payload.");
    }

    public async Task<StatusSummaryDto> CreateStatusAsync(string accessToken, Guid organizationId, CreateStatusRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/organizations/{organizationId}/statuses")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StatusSummaryDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a status payload.");
    }

    public async Task<StatusSummaryDto> UpdateStatusAsync(string accessToken, Guid statusId, UpdateStatusRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/statuses/{statusId}")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StatusSummaryDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return a status payload.");
    }

    public async Task<PagedResultDto<IdeaListItemDto>> ListIdeasPagedAsync(
        string accessToken,
        Guid boardId,
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

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/boards/{boardId}/ideas?{string.Join("&", query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResultDto<IdeaListItemDto>>(cancellationToken: cancellationToken)
            ?? new PagedResultDto<IdeaListItemDto>();
    }

    public async Task<PagedResultDto<IdeaListItemDto>> ListMyIdeasAsync(
        string accessToken,
        Guid organizationId,
        int page,
        int pageSize,
        string? search,
        string? filter,
        CancellationToken cancellationToken)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query.Add($"filter={Uri.EscapeDataString(filter)}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/organizations/{organizationId}/ideas?{string.Join("&", query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResultDto<IdeaListItemDto>>(cancellationToken: cancellationToken)
            ?? new PagedResultDto<IdeaListItemDto>();
    }

    public async Task<IReadOnlyList<IdeaListItemDto>> ListAllIdeasForOrgAsync(string accessToken, Guid organizationId, CancellationToken cancellationToken)
    {
        var boards = await ListBoardsAsync(accessToken, organizationId, cancellationToken);
        var allIdeas = new List<IdeaListItemDto>();

        foreach (var board in boards)
        {
            var result = await ListIdeasAsync(accessToken, board.BoardId, cancellationToken);
            allIdeas.AddRange(result.Items);
        }

        return allIdeas;
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string uri, string accessToken, object? content = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (content is not null)
        {
            request.Content = JsonContent.Create(content);
        }

        return request;
    }

    private async Task<TPayload> SendForPayloadAsync<TPayload>(
        HttpMethod method,
        string uri,
        string accessToken,
        object content,
        string payloadName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(method, uri, accessToken, content);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TPayload>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"The API did not return a {payloadName} payload.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    private async Task SendForNoContentAsync(
        HttpMethod method,
        string uri,
        string accessToken,
        object? content,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(method, uri, accessToken, content);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return;
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    private static async Task<AuthApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsDto>(cancellationToken: cancellationToken);
            if (validationProblem?.Errors is { Count: > 0 })
            {
                var detail = string.Join(" ", validationProblem.Errors.SelectMany(item => item.Value));
                return new AuthValidationException(
                    validationProblem.Title ?? "Validation failed.",
                    detail,
                    validationProblem.Errors);
            }
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken: cancellationToken);
        return new AuthApiException(
            (int)response.StatusCode,
            problem?.Title ?? "Idea fields error",
            problem?.Detail ?? "An idea fields API error occurred.");
    }
}
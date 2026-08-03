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
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<IdeaDetailDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API did not return an idea payload.");
    }

    public async Task<IdeaDetailDto> UpdateIdeaAsync(string accessToken, Guid ideaId, IdeaWriteRequestDto request, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/ideas/{ideaId}")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

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
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }
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

    public async Task ReorderSwimlanesAsync(string accessToken, Guid boardId, IReadOnlyList<Guid> orderedStatusIds, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/v1/boards/{boardId}/swimlanes/reorder")
        {
            Content = JsonContent.Create(new ReorderSwimlanesRequestDto { OrderedStatusIds = orderedStatusIds })
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IdeaListItemDto>> ListAllIdeasForBoardAsync(string accessToken, Guid boardId, CancellationToken cancellationToken)
    {
        const int PageSize = 100;

        var allIdeas = new List<IdeaListItemDto>();
        var page = 1;

        while (true)
        {
            var result = await ListIdeasPagedAsync(accessToken, boardId, page, PageSize, null, cancellationToken);
            if (result.Items.Count == 0)
            {
                break;
            }

            allIdeas.AddRange(result.Items);

            if (allIdeas.Count >= result.TotalCount || result.Items.Count < result.PageSize)
            {
                break;
            }

            page++;
        }

        return allIdeas;
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
            problem?.Title ?? "Workflow error",
            problem?.Detail ?? "A workflow API error occurred.");
    }
}

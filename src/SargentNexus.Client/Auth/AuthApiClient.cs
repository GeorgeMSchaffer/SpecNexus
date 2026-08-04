using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SargentNexus.Client.Auth;

public sealed class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken);
            return payload ?? new LoginResponseDto();
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task ChangePasswordAsync(string accessToken, ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/change-password")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return;
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<AuthenticatedUserDto> UpdateProfileAsync(string accessToken, UpdateProfileRequestDto request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, "api/v1/auth/me")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>(cancellationToken: cancellationToken);
            return payload ?? throw new AuthApiException(500, "Invalid profile response.", "The profile update response was empty.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/me");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>(cancellationToken: cancellationToken);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task RegisterAsync(SelfRegistrationRequestDto request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            return;
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
            problem?.Title ?? "Authentication error",
            problem?.Detail ?? "An authentication API error occurred.");
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using SargentNexus.Client.Auth;
using SargentNexus.Client.Workflow;

namespace SargentNexus.API.Tests;

public sealed class WorkflowApiClientTests
{
    [Theory]
    [InlineData("2026-08-06T13:00:00Z", "2026-08-06T23:00:00Z", "0 days ago")]
    [InlineData("2026-08-05T23:00:00Z", "2026-08-06T13:00:00Z", "1 day ago")]
    [InlineData("2026-08-03T12:00:00Z", "2026-08-06T13:00:00Z", "3 days ago")]
    [InlineData("2026-08-07T12:00:00Z", "2026-08-06T13:00:00Z", "0 days ago")]
    public void LocalDayAgeFormatter_UsesCalendarDaysAndClampsFuture(
        string createdAtUtc,
        string nowUtc,
        string expected)
    {
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("Test", TimeSpan.FromHours(-4), "Test", "Test");

        var result = LocalDayAgeFormatter.Format(
            DateTime.Parse(createdAtUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
            DateTimeOffset.Parse(nowUtc),
            timeZone);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task UpdateIdeaAsync_SendsExpandedIdeaWriteContract()
    {
        var ideaId = Guid.NewGuid();
        var ideaTypeId = Guid.NewGuid();
        var businessImpactId = Guid.NewGuid();
        var assigneeIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""
                {
                  "ideaId": "{{ideaId}}",
                  "title": "Updated",
                  "ideaTypeId": "{{ideaTypeId}}",
                  "businessImpactId": "{{businessImpactId}}"
                }
                """, Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        await client.UpdateIdeaAsync(
            "test-token",
            ideaId,
            new IdeaWriteRequestDto
            {
                Title = "Updated",
                Description = "Description",
                Priority = "High",
                IdeaTypeId = ideaTypeId,
                BusinessImpactId = businessImpactId,
                AssigneeUserIds = assigneeIds,
                TagNames = new[] { "Alpha", "Beta" }
            },
            CancellationToken.None);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal($"api/v1/ideas/{ideaId}", handler.RequestUri);
        using var payload = JsonDocument.Parse(handler.Body!);
        Assert.Equal(ideaTypeId, payload.RootElement.GetProperty("ideaTypeId").GetGuid());
        Assert.Equal(businessImpactId, payload.RootElement.GetProperty("businessImpactId").GetGuid());
        Assert.Equal(assigneeIds, payload.RootElement.GetProperty("assigneeUserIds").EnumerateArray().Select(item => item.GetGuid()));
        Assert.False(payload.RootElement.TryGetProperty("assigneeUserId", out _));
    }

    [Fact]
    public async Task ReorderIdeaTypesAsync_WithActiveAndArchivedIds_SendsCompleteOrderedList()
    {
        var organizationId = Guid.NewGuid();
        var activeId = Guid.NewGuid();
        var archivedId = Guid.NewGuid();
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = CreateClient(handler);

        await client.ReorderIdeaTypesAsync(
            "test-token",
            organizationId,
            new ReorderIdeaTypesRequestDto
            {
                OrderedIdeaTypeIds = new[] { archivedId, activeId }
            },
            CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal($"api/v1/organizations/{organizationId}/idea-types/reorder", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("test-token", handler.AuthorizationParameter);

        using var payload = JsonDocument.Parse(handler.Body!);
        Assert.Equal(
            new[] { archivedId, activeId },
            payload.RootElement.GetProperty("orderedIdeaTypeIds").EnumerateArray().Select(item => item.GetGuid()));
    }

    [Fact]
    public async Task DeleteBusinessImpactAsync_WhenServerRejectsLastActiveOption_PreservesValidationMessage()
    {
        var businessImpactId = Guid.NewGuid();
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """
                {
                  "title": "One or more validation errors occurred.",
                  "status": 400,
                  "errors": {
                    "workflow": ["The last active Business Impact cannot be deleted."]
                  }
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<AuthValidationException>(() =>
            client.DeleteBusinessImpactAsync("test-token", businessImpactId, CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("The last active Business Impact cannot be deleted.", exception.Detail);
        Assert.Equal(
            new[] { "The last active Business Impact cannot be deleted." },
            exception.Errors["workflow"]);
    }

    private static WorkflowApiClient CreateClient(HttpMessageHandler handler)
    {
        return new WorkflowApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sargentnexus.test/")
        });
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpMethod? Method { get; private set; }

        public string? RequestUri { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri?.PathAndQuery.TrimStart('/');
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return _responseFactory(request);
        }
    }
}
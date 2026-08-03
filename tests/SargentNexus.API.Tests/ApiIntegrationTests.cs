using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SargentNexus.Infrastructure;

namespace SargentNexus.API.Tests;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Login_WithSeededSiteAdmin_ReturnsAccessTokenAndRequiresPasswordChange()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "Abc123!Demo"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = payload.RootElement;

        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("accessToken").GetString()));
        Assert.True(root.GetProperty("requiresPasswordChange").GetBoolean());
        Assert.Equal("siteadmin@sargentnexus.local", root.GetProperty("user").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsProblemDetails401()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Invalid credentials.", problem!.Title);
    }

    [Fact]
    public async Task Login_AfterFiveInvalidAttempts_ReturnsTooManyRequests()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        HttpStatusCode? fifthStatus = null;
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "demo.acme.user@sargentnexus.local",
                password = "wrong-password"
            });

            if (attempt < 5)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
            else
            {
                fifthStatus = response.StatusCode;
            }
        }

        var sixth = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "demo.acme.user@sargentnexus.local",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.TooManyRequests, fifthStatus);
        Assert.Equal(HttpStatusCode.TooManyRequests, sixth.StatusCode);

        var problem = await sixth.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("User account is locked out.", problem!.Title);
    }

    [Fact]
    public async Task ProtectedOrganizationsEndpoint_WithoutToken_Returns401()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/organizations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedOrganizationsEndpoint_WithoutToken_ReturnsProblemDetailsContract()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/organizations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem!.Status);
        Assert.Equal("Authentication required.", problem.Title);
        Assert.Equal("A valid bearer token is required.", problem.Detail);
    }

    [Fact]
    public async Task Register_WithMissingRequiredFields_ReturnsValidationProblemDetailsContract()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            inviteCode = "",
            firstName = "",
            lastName = "",
            email = "",
            password = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.Equal("One or more validation errors occurred.", problem.Title);
        Assert.NotEmpty(problem.Errors);
        Assert.Contains(problem.Errors.Keys, key => string.Equals(key, "InviteCode", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(problem.Errors.Keys, key => string.Equals(key, "Email", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(problem.Errors.Keys, key => string.Equals(key, "Password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task OrgAdmin_CannotListOrganizations_ButCanReadOwnBoardIdeas()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "demo.acme.orgadmin@sargentnexus.local",
            password = "abc123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginPayload = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var token = loginPayload.RootElement.GetProperty("accessToken").GetString();
        var organizationId = loginPayload.RootElement.GetProperty("user").GetProperty("organizationId").GetGuid();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var organizationsResponse = await client.GetAsync("/api/v1/organizations?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, organizationsResponse.StatusCode);

        var boardsResponse = await client.GetAsync($"/api/v1/organizations/{organizationId}/boards");
        Assert.Equal(HttpStatusCode.OK, boardsResponse.StatusCode);

        using var boardsPayload = await JsonDocument.ParseAsync(await boardsResponse.Content.ReadAsStreamAsync());
        var boards = boardsPayload.RootElement;
        Assert.True(boards.ValueKind == JsonValueKind.Array);
        Assert.True(boards.GetArrayLength() > 0);

        var firstBoardId = boards[0].GetProperty("boardId").GetGuid();
        var ideasResponse = await client.GetAsync($"/api/v1/boards/{firstBoardId}/ideas?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, ideasResponse.StatusCode);

        using var ideasPayload = await JsonDocument.ParseAsync(await ideasResponse.Content.ReadAsStreamAsync());
        Assert.True(ideasPayload.RootElement.TryGetProperty("items", out var items));
        Assert.True(items.ValueKind == JsonValueKind.Array);
        Assert.True(items.GetArrayLength() > 0);
    }

    private sealed class IntegrationApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"SargentNexusApiIntegration_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Seed:SiteAdminPassword"] = "Abc123!Demo"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<SargentNexusDbContext>>();
                services.RemoveAll<SargentNexusDbContext>();
                services.AddDbContext<SargentNexusDbContext>(options => options.UseInMemoryDatabase(_dbName));
            });
        }
    }
}

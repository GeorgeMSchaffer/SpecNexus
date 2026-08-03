using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SargentNexus.Domain;
using SargentNexus.Infrastructure;

namespace SargentNexus.API.Tests;

public sealed class ApiIntegrationTests
{
    private static readonly string[] DefaultStatusNames =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

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

    [Fact]
    public async Task DevelopmentStartup_SeedsExpectedDemoGraph()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var demoLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "demo.acme.user@sargentnexus.local",
            password = "abc123!"
        });

        Assert.Equal(HttpStatusCode.OK, demoLogin.StatusCode);

        using var loginPayload = await JsonDocument.ParseAsync(await demoLogin.Content.ReadAsStreamAsync());
        Assert.True(loginPayload.RootElement.GetProperty("requiresPasswordChange").GetBoolean());

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        Assert.Equal(4, await dbContext.Organizations.CountAsync());
        Assert.Equal(10, await dbContext.Users.CountAsync());
        Assert.Equal(3, await dbContext.Users.CountAsync(item => item.Role == UserRole.OrgAdmin));
        Assert.Equal(3, await dbContext.Users.CountAsync(item => item.Role == UserRole.User));
        Assert.Equal(3, await dbContext.Users.CountAsync(item => item.Role == UserRole.ReadOnly));
        Assert.Equal(3, await dbContext.Boards.CountAsync());
        Assert.Equal(15, await dbContext.Statuses.CountAsync());
        Assert.Equal(15, await dbContext.BoardSwimlanes.CountAsync());
        Assert.Equal(15, await dbContext.Ideas.CountAsync());
        Assert.Equal(30, await dbContext.Comments.CountAsync());

        var organizations = await dbContext.Organizations.OrderBy(item => item.CompanyName).ToListAsync();
        var boards = await dbContext.Boards.ToListAsync();
        var demoOrganizationIds = boards.Select(item => item.OrganizationId).ToHashSet();

        Assert.Single(organizations.Where(item => !demoOrganizationIds.Contains(item.Id)));
        Assert.Equal(3, demoOrganizationIds.Count);

        foreach (var organization in organizations.Where(item => demoOrganizationIds.Contains(item.Id)))
        {
            Assert.False(string.IsNullOrWhiteSpace(organization.InviteCode));

            var board = boards.Single(item => item.OrganizationId == organization.Id);
            var statuses = await dbContext.Statuses
                .Where(item => item.OrganizationId == organization.Id)
                .OrderBy(item => item.Name)
                .ToListAsync();
            var swimlanes = await dbContext.BoardSwimlanes
                .Where(item => item.BoardId == board.Id)
                .OrderBy(item => item.Order)
                .ToListAsync();
            var ideas = await dbContext.Ideas
                .Where(item => item.BoardId == board.Id)
                .ToListAsync();
            var ideaIds = ideas.Select(item => item.Id).ToList();
            var comments = await dbContext.Comments
                .Where(item => ideaIds.Contains(item.IdeaId))
                .ToListAsync();

            Assert.Equal(DefaultStatusNames.OrderBy(item => item).ToArray(), statuses.Select(item => item.Name).OrderBy(item => item).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, swimlanes.Select(item => item.Order).ToArray());
            Assert.Equal(DefaultStatusNames, swimlanes.Select(item => statuses.Single(status => status.Id == item.StatusId).Name).ToArray());
            Assert.Equal(5, ideas.Count);
            Assert.Equal(5, ideas.Select(item => item.StatusId).Distinct().Count());
            Assert.Equal(10, comments.Count);
        }
    }

    [Fact]
    public async Task ProductionStartup_SeedsSiteAdminOnly_AndSuppressesDemoEnvironment()
    {
        await using var factory = new IntegrationApiFactory("Production");
        using var client = factory.CreateClient();

        var siteAdminLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "Abc123!Demo"
        });

        Assert.Equal(HttpStatusCode.OK, siteAdminLogin.StatusCode);

        var demoUserLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "demo.acme.user@sargentnexus.local",
            password = "abc123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, demoUserLogin.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        Assert.Equal(1, await dbContext.Organizations.CountAsync());
        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(0, await dbContext.Users.CountAsync(item => item.Role != UserRole.SiteAdmin));
        Assert.Equal(0, await dbContext.Boards.CountAsync());
        Assert.Equal(0, await dbContext.Ideas.CountAsync());
        Assert.Equal(0, await dbContext.Comments.CountAsync());
    }

    [Fact]
    public async Task DevelopmentStartup_RestartTwice_DoesNotDuplicateSeededGraph()
    {
        var sharedDatabaseName = $"SargentNexusApiIntegrationRestart_{Guid.NewGuid():N}";

        await using (var firstFactory = new IntegrationApiFactory(databaseName: sharedDatabaseName))
        {
            using var firstClient = firstFactory.CreateClient();
            var firstHealth = await firstClient.GetAsync("/api/v1/health");
            Assert.Equal(HttpStatusCode.OK, firstHealth.StatusCode);
        }

        await using var secondFactory = new IntegrationApiFactory(databaseName: sharedDatabaseName);
        using var secondClient = secondFactory.CreateClient();
        var secondHealth = await secondClient.GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.OK, secondHealth.StatusCode);

        await using var scope = secondFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        Assert.Equal(4, await dbContext.Organizations.CountAsync());
        Assert.Equal(10, await dbContext.Users.CountAsync());
        Assert.Equal(3, await dbContext.Boards.CountAsync());
        Assert.Equal(15, await dbContext.Statuses.CountAsync());
        Assert.Equal(15, await dbContext.BoardSwimlanes.CountAsync());
        Assert.Equal(15, await dbContext.Ideas.CountAsync());
        Assert.Equal(30, await dbContext.Comments.CountAsync());
    }

    [Fact]
    public async Task CreateOrganization_WithSiteAdmin_ProvisionsDefaultsAndPersistsAuditEvent()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var token = await LoginAndGetTokenAsync(client, "siteadmin@sargentnexus.local", "Abc123!Demo");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "Northwind Labs",
            address = "123 Demo St",
            city = "Seattle",
            state = "WA",
            zip = "98101",
            phone = "206-555-0101",
            primaryContactFirstName = "Nora",
            primaryContactLastName = "West"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var organizationId = payload.RootElement.GetProperty("organizationId").GetGuid();
        var defaultBoardId = payload.RootElement.GetProperty("defaultBoardId").GetGuid();
        var defaultStatusCount = payload.RootElement.GetProperty("defaultStatusCount").GetInt32();
        var inviteCode = payload.RootElement.GetProperty("inviteCode").GetString();

        Assert.Equal(5, defaultStatusCount);
        Assert.False(string.IsNullOrWhiteSpace(inviteCode));

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        var organization = await dbContext.Organizations.SingleAsync(item => item.Id == organizationId);
        var statuses = await dbContext.Statuses.Where(item => item.OrganizationId == organizationId).ToListAsync();
        var board = await dbContext.Boards.SingleAsync(item => item.Id == defaultBoardId);
        var swimlaneCount = await dbContext.BoardSwimlanes.CountAsync(item => item.BoardId == defaultBoardId);
        var auditEvent = await dbContext.AuditEvents.SingleAsync(item =>
            item.EventType == "Administration.OrganizationCreated" && item.EntityId == organizationId);

        Assert.Equal("Northwind Labs", organization.CompanyName);
        Assert.Equal(5, statuses.Count);
        Assert.Equal(organizationId, board.OrganizationId);
        Assert.Equal(5, swimlaneCount);
        Assert.Equal(organizationId, auditEvent.OrganizationId);
        Assert.Contains("\"DefaultStatusCount\":5", auditEvent.Metadata, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Register_WithInviteCode_CreatesUserInProvisionedOrganization()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var siteAdminToken = await LoginAndGetTokenAsync(client, "siteadmin@sargentnexus.local", "Abc123!Demo");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", siteAdminToken);

        var organizationResponse = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "Fabrikam Labs",
            address = "456 Launch Ave",
            city = "Portland",
            state = "OR",
            zip = "97204",
            phone = "503-555-0101",
            primaryContactFirstName = "Casey",
            primaryContactLastName = "North"
        });

        Assert.Equal(HttpStatusCode.Created, organizationResponse.StatusCode);

        using var organizationPayload = await JsonDocument.ParseAsync(await organizationResponse.Content.ReadAsStreamAsync());
        var organizationId = organizationPayload.RootElement.GetProperty("organizationId").GetGuid();
        var inviteCode = organizationPayload.RootElement.GetProperty("inviteCode").GetString();
        Assert.False(string.IsNullOrWhiteSpace(inviteCode));

        client.DefaultRequestHeaders.Authorization = null;

        var registrationResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            inviteCode,
            firstName = "Taylor",
            lastName = "Morgan",
            email = "taylor.morgan@sargentnexus.local",
            password = "Abc123!Join"
        });

        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);

        using var registrationPayload = await JsonDocument.ParseAsync(await registrationResponse.Content.ReadAsStreamAsync());
        Assert.Equal(organizationId, registrationPayload.RootElement.GetProperty("organizationId").GetGuid());
        Assert.Equal("taylor.morgan@sargentnexus.local", registrationPayload.RootElement.GetProperty("email").GetString());

        var token = await LoginAndGetTokenAsync(client, "taylor.morgan@sargentnexus.local", "Abc123!Join");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var meResponse = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using var mePayload = await JsonDocument.ParseAsync(await meResponse.Content.ReadAsStreamAsync());
        var me = mePayload.RootElement;
        Assert.Equal(organizationId, me.GetProperty("organizationId").GetGuid());
        Assert.Equal("User", me.GetProperty("role").GetString());

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();
        var registeredUser = await dbContext.Users.SingleAsync(item => item.Email == "taylor.morgan@sargentnexus.local");

        Assert.Equal(organizationId, registeredUser.OrganizationId);
        Assert.Equal(UserRole.User, registeredUser.Role);
        Assert.False(registeredUser.MustChangePassword);
    }

    [Fact]
    public async Task Login_SuccessAndFailure_PersistAuditEvents()
    {
        await using var factory = new IntegrationApiFactory();
        using var client = factory.CreateClient();

        var success = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "Abc123!Demo"
        });
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);

        var failure = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "wrong-password"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, failure.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        Assert.Contains(dbContext.AuditEvents, item => item.EventType == "Authentication.LoginSucceeded");
        Assert.Contains(dbContext.AuditEvents, item => item.EventType == "Authentication.LoginFailed");
    }

    private static async Task<string> LoginAndGetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return payload.RootElement.GetProperty("accessToken").GetString()
            ?? throw new Xunit.Sdk.XunitException("Expected access token in login response.");
    }

    private sealed class IntegrationApiFactory : WebApplicationFactory<Program>
    {
        private static readonly ConcurrentDictionary<string, InMemoryDatabaseRoot> SharedDatabaseRoots = new(StringComparer.Ordinal);

        private readonly string _dbName;
        private readonly string _environmentName;

        public IntegrationApiFactory(string environmentName = "Development", string? databaseName = null)
        {
            _dbName = databaseName ?? $"SargentNexusApiIntegration_{Guid.NewGuid():N}";
            _environmentName = environmentName;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environmentName);
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
                var databaseRoot = SharedDatabaseRoots.GetOrAdd(_dbName, _ => new InMemoryDatabaseRoot());
                services.AddDbContext<SargentNexusDbContext>(options => options.UseInMemoryDatabase(_dbName, databaseRoot));
            });
        }
    }
}

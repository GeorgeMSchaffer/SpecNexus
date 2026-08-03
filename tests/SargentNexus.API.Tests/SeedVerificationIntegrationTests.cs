using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SargentNexus.Infrastructure;

namespace SargentNexus.API.Tests;

/// <summary>
/// T050: End-to-end seed verification covering organization bootstrap defaults,
/// invite code generation, audit event persistence, and deferred-scope boundaries
/// (OAuth and SAML remain unimplemented in MVP).
/// </summary>
public sealed class SeedVerificationIntegrationTests
{
    // ── Organization bootstrap via API ────────────────────────────────────────

    [Fact]
    public async Task SiteAdmin_CreateOrganization_ReturnsCreatedWithFiveDefaultStatuses()
    {
        await using var factory = new SeedVerificationApiFactory();
        var token = await LoginAsSiteAdminAsync(factory);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "Bootstrap Test Corp",
            address = "1 Main Street",
            city = "Portland",
            state = "OR",
            zip = "97201",
            phone = "503-555-0100",
            primaryContactFirstName = "Jane",
            primaryContactLastName = "Doe"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = payload.RootElement;

        Assert.Equal(5, root.GetProperty("defaultStatusCount").GetInt32());
        Assert.NotEqual(Guid.Empty, root.GetProperty("defaultBoardId").GetGuid());
        Assert.NotEqual(Guid.Empty, root.GetProperty("organizationId").GetGuid());
    }

    [Fact]
    public async Task SiteAdmin_CreateOrganization_ResponseIncludesNonEmptyInviteCode()
    {
        await using var factory = new SeedVerificationApiFactory();
        var token = await LoginAsSiteAdminAsync(factory);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "Invite Code Test LLC",
            address = "2 Oak Lane",
            city = "Phoenix",
            state = "AZ",
            zip = "85001",
            phone = "602-555-0200",
            primaryContactFirstName = "Mark",
            primaryContactLastName = "Chen"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var inviteCode = payload.RootElement.GetProperty("inviteCode").GetString();

        Assert.False(string.IsNullOrWhiteSpace(inviteCode), "Created organization must include a non-empty invite code in the API response.");
    }

    [Fact]
    public async Task SiteAdmin_CreateOrganization_PersistsOrganizationAuditEvent()
    {
        await using var factory = new SeedVerificationApiFactory();
        var token = await LoginAsSiteAdminAsync(factory);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "Audit Test Ltd",
            address = "3 Birch Boulevard",
            city = "Nashville",
            state = "TN",
            zip = "37201",
            phone = "615-555-0300",
            primaryContactFirstName = "Sara",
            primaryContactLastName = "Park"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        var orgAuditEvent = await dbContext.AuditEvents
            .SingleOrDefaultAsync(e => e.EventType == "Administration.OrganizationCreated");

        Assert.NotNull(orgAuditEvent);
        Assert.Equal("Organization", orgAuditEvent!.EntityType);
        Assert.NotNull(orgAuditEvent.OrganizationId);
    }

    [Fact]
    public async Task SiteAdmin_CreateOrganization_DefaultBoardAndStatusesExistInDatabase()
    {
        await using var factory = new SeedVerificationApiFactory();
        var token = await LoginAsSiteAdminAsync(factory);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/v1/organizations", new
        {
            companyName = "DB Validation Corp",
            address = "4 Cedar Court",
            city = "Columbus",
            state = "OH",
            zip = "43201",
            phone = "614-555-0400",
            primaryContactFirstName = "Tom",
            primaryContactLastName = "Rivers"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var organizationId = payload.RootElement.GetProperty("organizationId").GetGuid();
        var defaultBoardId = payload.RootElement.GetProperty("defaultBoardId").GetGuid();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        var statusCount = await dbContext.Statuses
            .CountAsync(s => s.OrganizationId == organizationId && !s.IsDeleted);

        var board = await dbContext.Boards
            .SingleOrDefaultAsync(b => b.Id == defaultBoardId && b.OrganizationId == organizationId);

        var swimlaneCount = await dbContext.BoardSwimlanes
            .CountAsync(sw => sw.BoardId == defaultBoardId);

        Assert.Equal(5, statusCount);
        Assert.NotNull(board);
        Assert.Equal(5, swimlaneCount);
    }

    // ── Seeded demo organization invite codes ─────────────────────────────────

    [Fact]
    public async Task Development_SeededDemoOrganizations_EachHaveNonEmptyInviteCode()
    {
        await using var factory = new SeedVerificationApiFactory();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SargentNexusDbContext>();

        var demoOrganizationNames = new[]
        {
            "Acme Advisory Group",
            "Northwind Services",
            "Summit Dynamics"
        };

        foreach (var name in demoOrganizationNames)
        {
            var org = await dbContext.Organizations.SingleOrDefaultAsync(o => o.CompanyName == name);
            Assert.NotNull(org);
            Assert.False(string.IsNullOrWhiteSpace(org!.InviteCode),
                $"Demo organization '{name}' must have a non-empty invite code.");
            Assert.NotEqual(default, org.InviteCodeGeneratedAtUtc);
        }
    }

    // ── Site Admin seed first-login behavior ──────────────────────────────────

    [Fact]
    public async Task SeededSiteAdmin_FirstLogin_IsForcedThroughPasswordChange()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "Abc123!Demo"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.GetProperty("requiresPasswordChange").GetBoolean(),
            "Seeded Site Admin must be forced through password change on first login.");
    }

    // ── Deferred-scope boundary: OAuth not implemented ─────────────────────────

    [Fact]
    public async Task DeferredScope_OAuthChallengeEndpoint_ReturnsNotFound()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/oauth/challenge", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeferredScope_OAuthCallbackEndpoint_ReturnsNotFound()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/oauth/callback");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Deferred-scope boundary: SAML not implemented ─────────────────────────

    [Fact]
    public async Task DeferredScope_SamlLoginEndpoint_ReturnsNotFound()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/saml/login", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeferredScope_SamlCallbackEndpoint_ReturnsNotFound()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/saml/callback", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Deferred-scope boundary: DI does not include OAuth or SAML services ───

    [Fact]
    public async Task DeferredScope_OAuthConfigEndpoint_ReturnsNotFound()
    {
        await using var factory = new SeedVerificationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/organizations/" + Guid.NewGuid() + "/oauth/config");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string> LoginAsSiteAdminAsync(SeedVerificationApiFactory factory)
    {
        // Site Admin has MustChangePassword=true; change it first so the token is unrestricted.
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "Abc123!Demo"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginPayload = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var token = loginPayload.RootElement.GetProperty("accessToken").GetString()!;

        // Change password so Site Admin can perform admin operations.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var changeResponse = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "Abc123!Demo",
            newPassword = "SeedTest1!Admin"
        });

        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        // Login again with new password to get a clean token.
        client.DefaultRequestHeaders.Authorization = null;
        var freshLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "siteadmin@sargentnexus.local",
            password = "SeedTest1!Admin"
        });

        Assert.Equal(HttpStatusCode.OK, freshLogin.StatusCode);

        using var freshPayload = await JsonDocument.ParseAsync(await freshLogin.Content.ReadAsStreamAsync());
        return freshPayload.RootElement.GetProperty("accessToken").GetString()!;
    }

    private sealed class SeedVerificationApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"SeedVerification_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
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

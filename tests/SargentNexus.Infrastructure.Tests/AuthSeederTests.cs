using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SargentNexus.Application.Auth;
using SargentNexus.Domain;
using SargentNexus.Infrastructure;

public sealed class AuthSeederTests
{
    private static readonly (string Name, string Color)[] ExpectedBusinessImpacts =
    {
        ("Low", "#16A34A"),
        ("Medium", "#2563EB"),
        ("High", "#D97706"),
        ("Critical", "#DC2626")
    };

    private static readonly string[] ExpectedIdeaTypeNames =
    {
        "Continuous Improvement",
        "Process Revision"
    };

    private static readonly string[] DefaultStatusNames =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_CreatesExpectedDemoGraph()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var organizations = await dbContext.Organizations.CountAsync();
        var boards = await dbContext.Boards.CountAsync();
        var swimlanes = await dbContext.BoardSwimlanes.CountAsync();
        var ideas = await dbContext.Ideas.CountAsync();
        var comments = await dbContext.Comments.CountAsync();

        var orgAdmins = await dbContext.Users.CountAsync(item => item.Role == UserRole.OrgAdmin);
        var users = await dbContext.Users.CountAsync(item => item.Role == UserRole.User);
        var readOnlyUsers = await dbContext.Users.CountAsync(item => item.Role == UserRole.ReadOnly);

        Assert.Equal(3, organizations);
        Assert.Equal(3, boards);
        Assert.Equal(15, swimlanes);
        Assert.Equal(15, ideas);
        Assert.Equal(30, comments);
        Assert.Equal(3, orgAdmins);
        Assert.Equal(3, users);
        Assert.Equal(3, readOnlyUsers);

        var seededUsers = await dbContext.Users
            .Where(item => item.Role != UserRole.SiteAdmin)
            .ToListAsync();

        Assert.All(seededUsers, item =>
        {
            Assert.False(item.MustChangePassword);
            Assert.Equal(UserLifecycleStatus.Active, item.Status);
            Assert.True(hasher.Verify("abc123!", item.PasswordHash));
        });
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_LegacyDemoPasswordPrompt_ClearsPrompt()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var demoUser = await dbContext.Users.FirstAsync(item => item.Role != UserRole.SiteAdmin);
        demoUser.MustChangePassword = true;
        await dbContext.SaveChangesAsync();

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        Assert.False(demoUser.MustChangePassword);
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_PendingTemporaryPassword_PreservesPrompt()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var demoUser = await dbContext.Users.FirstAsync(item => item.Role != UserRole.SiteAdmin);
        var expiresAtUtc = DateTime.UtcNow.AddHours(1);
        demoUser.MustChangePassword = true;
        demoUser.TemporaryPasswordHash = hasher.Hash("TemporaryPassword1!");
        demoUser.TemporaryPasswordExpiresAtUtc = expiresAtUtc;
        await dbContext.SaveChangesAsync();

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        Assert.True(demoUser.MustChangePassword);
        Assert.NotNull(demoUser.TemporaryPasswordHash);
        Assert.Equal(expiresAtUtc, demoUser.TemporaryPasswordExpiresAtUtc);
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_RunTwice_IsIdempotent()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);
        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        Assert.Equal(3, await dbContext.Organizations.CountAsync());
        Assert.Equal(9, await dbContext.Users.CountAsync(item => item.Role != UserRole.SiteAdmin));
        Assert.Equal(3, await dbContext.Boards.CountAsync());
        Assert.Equal(15, await dbContext.BoardSwimlanes.CountAsync());
        Assert.Equal(15, await dbContext.Ideas.CountAsync());
        Assert.Equal(30, await dbContext.Comments.CountAsync());
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_CreatesCanonicalIdeaFields()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var organizations = await dbContext.Organizations.ToListAsync();
        foreach (var organization in organizations)
        {
            var ideaTypes = await dbContext.IdeaTypes
                .Where(item => item.OrganizationId == organization.Id)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();
            var businessImpacts = await dbContext.BusinessImpacts
                .Where(item => item.OrganizationId == organization.Id)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();

            Assert.Equal(ExpectedIdeaTypeNames, ideaTypes.Select(item => item.Name));
            Assert.Equal(ExpectedBusinessImpacts.Select(item => item.Name), businessImpacts.Select(item => item.Name));
            Assert.Equal(ExpectedBusinessImpacts.Select(item => item.Color), businessImpacts.Select(item => item.Color));
        }
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_Rerun_PreservesAdministratorManagedIdeaFields()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var ideaType = await dbContext.IdeaTypes.OrderBy(item => item.SortOrder).FirstAsync();
        var businessImpact = await dbContext.BusinessImpacts.OrderBy(item => item.SortOrder).FirstAsync();
        ideaType.Name = "Administrator Type";
        ideaType.SortOrder = 7;
        ideaType.IsDeleted = true;
        businessImpact.Name = "Administrator Impact";
        businessImpact.Color = "#123456";
        businessImpact.SortOrder = 9;
        businessImpact.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        Assert.Equal("Administrator Type", ideaType.Name);
        Assert.Equal(7, ideaType.SortOrder);
        Assert.True(ideaType.IsDeleted);
        Assert.Equal("Administrator Impact", businessImpact.Name);
        Assert.Equal("#123456", businessImpact.Color);
        Assert.Equal(9, businessImpact.SortOrder);
        Assert.True(businessImpact.IsDeleted);
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_AssociatesSiteAdminWithAnOrganization()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var siteAdmin = await dbContext.Users.SingleAsync(item => item.Role == UserRole.SiteAdmin);
        var organizationIds = await dbContext.Organizations.Select(item => item.Id).ToListAsync();

        Assert.NotNull(siteAdmin.OrganizationId);
        Assert.Contains(siteAdmin.OrganizationId!.Value, organizationIds);
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_CreatesExpectedPerOrganizationGraphQuality()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var organizations = await dbContext.Organizations.ToListAsync();
        var boards = await dbContext.Boards.ToListAsync();
        var swimlanes = await dbContext.BoardSwimlanes.ToListAsync();
        var ideas = await dbContext.Ideas.ToListAsync();
        var comments = await dbContext.Comments.ToListAsync();
        var statuses = await dbContext.Statuses.ToListAsync();
        var users = await dbContext.Users.ToListAsync();

        foreach (var organization in organizations)
        {
            var organizationBoards = boards.Where(item => item.OrganizationId == organization.Id).ToList();
            var board = Assert.Single(organizationBoards);

            var boardSwimlanes = swimlanes
                .Where(item => item.BoardId == board.Id)
                .OrderBy(item => item.Order)
                .ToList();

            Assert.Equal(5, boardSwimlanes.Count);
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, boardSwimlanes.Select(item => item.Order).ToArray());

            var organizationStatuses = statuses.Where(item => item.OrganizationId == organization.Id).ToList();
            Assert.Equal(DefaultStatusNames.Length, organizationStatuses.Count);
            Assert.Equal(
                DefaultStatusNames.OrderBy(item => item).ToArray(),
                organizationStatuses.Select(item => item.Name).OrderBy(item => item).ToArray());

            var boardIdeas = ideas.Where(item => item.BoardId == board.Id).ToList();
            Assert.Equal(DefaultStatusNames.Length, boardIdeas.Count);
            Assert.Equal(DefaultStatusNames.Length, boardIdeas.Select(item => item.StatusId).Distinct().Count());

            var boardStatusIds = boardSwimlanes.Select(item => item.StatusId).ToHashSet();
            var statusById = organizationStatuses.ToDictionary(item => item.Id);

            var orderedStatusNames = boardSwimlanes
                .OrderBy(item => item.Order)
                .Select(item => statusById[item.StatusId].Name)
                .ToArray();

            Assert.Equal(DefaultStatusNames, orderedStatusNames);

            Assert.All(boardIdeas, idea =>
            {
                Assert.Equal(organization.Id, idea.OrganizationId);
                Assert.Contains(idea.StatusId, boardStatusIds);

                var ideaStatus = statuses.Single(item => item.Id == idea.StatusId);
                Assert.Equal(organization.Id, ideaStatus.OrganizationId);

                var ideaComments = comments.Where(item => item.IdeaId == idea.Id).ToList();
                Assert.Equal(2, ideaComments.Count);

                var authorRoles = ideaComments
                    .Select(item => users.Single(user => user.Id == item.AuthorUserId).Role)
                    .ToHashSet();

                Assert.Contains(UserRole.OrgAdmin, authorRoles);
                Assert.Contains(UserRole.ReadOnly, authorRoles);
            });
        }
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_RerunAfterMutation_RepairsGraphWithoutResettingUserAuthenticationState()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var organizationA = await dbContext.Organizations.OrderBy(item => item.CompanyName).FirstAsync();
        var organizationB = await dbContext.Organizations
            .Where(item => item.Id != organizationA.Id)
            .OrderBy(item => item.CompanyName)
            .FirstAsync();

        var boardA = await dbContext.Boards.SingleAsync(item => item.OrganizationId == organizationA.Id);
        var ideaOriginalStatus = await dbContext.Statuses
            .SingleAsync(item => item.OrganizationId == organizationA.Id && item.Name == "New / Pending");
        var ideaA = await dbContext.Ideas
            .SingleAsync(item => item.BoardId == boardA.Id && item.StatusId == ideaOriginalStatus.Id);

        var foreignStatus = await dbContext.Statuses
            .SingleAsync(item => item.OrganizationId == organizationB.Id && item.Name == "Complete");

        var statusToRestore = await dbContext.Statuses
            .SingleAsync(item => item.OrganizationId == organizationA.Id && item.Name == "In Review");

        var swimlaneToRepair = await dbContext.BoardSwimlanes
            .SingleAsync(item => item.BoardId == boardA.Id && item.StatusId == statusToRestore.Id);

        var readOnlyUserToReset = await dbContext.Users
            .SingleAsync(item => item.OrganizationId == organizationA.Id && item.Role == UserRole.ReadOnly);

        var changedPasswordHash = hasher.Hash("ChangedPassword1!");
        var temporaryPasswordExpiresAtUtc = DateTime.UtcNow.AddHours(1);

        ideaA.StatusId = foreignStatus.Id;
        statusToRestore.IsDeleted = true;
        swimlaneToRepair.Order = 99;
        readOnlyUserToReset.Status = UserLifecycleStatus.Inactive;
        readOnlyUserToReset.MustChangePassword = false;
        readOnlyUserToReset.PasswordHash = changedPasswordHash;
        readOnlyUserToReset.TemporaryPasswordHash = "temp-hash";
        readOnlyUserToReset.TemporaryPasswordExpiresAtUtc = temporaryPasswordExpiresAtUtc;

        await dbContext.SaveChangesAsync();

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var repairedStatus = await dbContext.Statuses
            .SingleAsync(item => item.OrganizationId == organizationA.Id && item.Name == "In Review");

        var repairedSwimlanes = await dbContext.BoardSwimlanes
            .Where(item => item.BoardId == boardA.Id)
            .OrderBy(item => item.Order)
            .ToListAsync();

        var repairedIdea = await dbContext.Ideas.SingleAsync(item => item.Id == ideaA.Id);
        var repairedIdeaStatus = await dbContext.Statuses.SingleAsync(item => item.Id == repairedIdea.StatusId);

        var repairedReadOnly = await dbContext.Users.SingleAsync(item => item.Id == readOnlyUserToReset.Id);

        Assert.False(repairedStatus.IsDeleted);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, repairedSwimlanes.Select(item => item.Order).ToArray());
        Assert.Equal(organizationA.Id, repairedIdeaStatus.OrganizationId);
        Assert.Equal(UserLifecycleStatus.Inactive, repairedReadOnly.Status);
        Assert.False(repairedReadOnly.MustChangePassword);
        Assert.Equal(changedPasswordHash, repairedReadOnly.PasswordHash);
        Assert.Equal("temp-hash", repairedReadOnly.TemporaryPasswordHash);
        Assert.Equal(temporaryPasswordExpiresAtUtc, repairedReadOnly.TemporaryPasswordExpiresAtUtc);

        Assert.Equal(3, await dbContext.Organizations.CountAsync());
        Assert.Equal(9, await dbContext.Users.CountAsync(item => item.Role != UserRole.SiteAdmin));
        Assert.Equal(3, await dbContext.Boards.CountAsync());
        Assert.Equal(15, await dbContext.Statuses.CountAsync());
        Assert.Equal(15, await dbContext.BoardSwimlanes.CountAsync());
        Assert.Equal(15, await dbContext.Ideas.CountAsync());
        Assert.Equal(30, await dbContext.Comments.CountAsync());
    }

    [Fact]
    public async Task SeedSiteAdminAsync_WithoutConfiguredPassword_UsesDefaultDemoPassword()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher, configuration: new ConfigurationManager());

        await seeder.SeedSiteAdminAsync(CancellationToken.None);

        var siteAdmin = await dbContext.Users.SingleAsync(item => item.Role == UserRole.SiteAdmin);

        Assert.True(hasher.Verify("Abc123!Demo", siteAdmin.PasswordHash));
    }

    [Fact]
    public async Task SeedSiteAdminAsync_WithInMemoryProvider_CreatesSiteAdmin()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedSiteAdminAsync(CancellationToken.None);

        var siteAdmin = await dbContext.Users.SingleAsync(item => item.Role == UserRole.SiteAdmin);

        Assert.Equal("siteadmin@sargentnexus.local", siteAdmin.Email);
        Assert.True(siteAdmin.MustChangePassword);
        Assert.True(hasher.Verify("Abc123!Demo", siteAdmin.PasswordHash));
    }

    [Fact]
    public async Task SeedSiteAdminAsync_ExistingUser_PreservesChangedPasswordAndClearedPrompt()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedSiteAdminAsync(CancellationToken.None);

        var siteAdmin = await dbContext.Users.SingleAsync(item => item.Role == UserRole.SiteAdmin);
        siteAdmin.PasswordHash = hasher.Hash("ChangedPassword1!");
        siteAdmin.MustChangePassword = false;
        await dbContext.SaveChangesAsync();

        await seeder.SeedSiteAdminAsync(CancellationToken.None);

        var reseededSiteAdmin = await dbContext.Users.SingleAsync(item => item.Role == UserRole.SiteAdmin);
        Assert.True(hasher.Verify("ChangedPassword1!", reseededSiteAdmin.PasswordHash));
        Assert.False(reseededSiteAdmin.MustChangePassword);
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_EachOrganizationHasNonEmptyInviteCode()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var organizations = await dbContext.Organizations.ToListAsync();
        Assert.Equal(3, organizations.Count);
        Assert.All(organizations, org =>
        {
            Assert.False(string.IsNullOrWhiteSpace(org.InviteCode), $"Organization '{org.CompanyName}' has an empty invite code.");
            Assert.NotEqual(default, org.InviteCodeGeneratedAtUtc);
        });
    }

    [Fact]
    public async Task SeedDevelopmentDemoEnvironmentAsync_FirstRun_InviteCodesAreUniqueAcrossOrganizations()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedDevelopmentDemoEnvironmentAsync(CancellationToken.None);

        var inviteCodes = await dbContext.Organizations
            .Select(org => org.InviteCode)
            .ToListAsync();

        Assert.Equal(inviteCodes.Count, inviteCodes.Distinct().Count());
    }

    [Fact]
    public async Task SeedSiteAdminAsync_WithInMemoryProvider_CreatesOrganizationWithNonEmptyInviteCode()
    {
        await using var dbContext = CreateDbContext();
        var hasher = CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");
        var seeder = CreateSeeder(dbContext, hasher);

        await seeder.SeedSiteAdminAsync(CancellationToken.None);

        var organization = await dbContext.Organizations.SingleAsync();
        Assert.False(string.IsNullOrWhiteSpace(organization.InviteCode), "Seeded Site Admin organization must have a non-empty invite code.");
        Assert.NotEqual(default, organization.InviteCodeGeneratedAtUtc);
    }

    [Fact]
    public void DbModel_CommentAuthorForeignKey_UsesRestrictDeleteBehavior()
    {
        using var dbContext = CreateDbContext();

        var commentEntityType = dbContext.Model.FindEntityType(typeof(Comment));
        Assert.NotNull(commentEntityType);

        var authorForeignKey = commentEntityType!
            .GetForeignKeys()
            .Single(item => item.PrincipalEntityType.ClrType == typeof(User) && item.Properties.Any(property => property.Name == nameof(Comment.AuthorUserId)));

        Assert.Equal(DeleteBehavior.Restrict, authorForeignKey.DeleteBehavior);
    }

    private static SargentNexusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SargentNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new SargentNexusDbContext(options);
    }

    private static IAuthSeeder CreateSeeder(SargentNexusDbContext dbContext, IPasswordHasher hasher, ConfigurationManager? configuration = null)
    {
        var effectiveConfiguration = configuration ?? new ConfigurationManager();

        if (configuration is null)
        {
            effectiveConfiguration["Seed:SiteAdminPassword"] = "Abc123!Demo";
        }

        var type = typeof(SargentNexusDbContext).Assembly.GetType("SargentNexus.Infrastructure.AuthSeeder", throwOnError: true)!;

        return (IAuthSeeder)(Activator.CreateInstance(type, dbContext, hasher, effectiveConfiguration)
            ?? throw new InvalidOperationException("Unable to construct AuthSeeder."));
    }

    private static T CreateInternal<T>(string fullTypeName) where T : class
    {
        var assembly = typeof(SargentNexusDbContext).Assembly;
        var type = assembly.GetType(fullTypeName, throwOnError: true)!;

        return (T)(Activator.CreateInstance(type, nonPublic: true)
            ?? throw new InvalidOperationException($"Unable to create type {fullTypeName}."));
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SargentNexus.Application.Auth;
using SargentNexus.Domain;
using SargentNexus.Infrastructure;

public sealed class AuthSeederTests
{
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
            Assert.True(item.MustChangePassword);
            Assert.Equal(UserLifecycleStatus.Active, item.Status);
            Assert.True(hasher.Verify("abc123!", item.PasswordHash));
        });
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

    private static IAuthSeeder CreateSeeder(SargentNexusDbContext dbContext, IPasswordHasher hasher)
    {
        var configuration = new ConfigurationManager
        {
            ["Seed:SiteAdminPassword"] = "Abc123!Demo"
        };

        var type = typeof(SargentNexusDbContext).Assembly.GetType("SargentNexus.Infrastructure.AuthSeeder", throwOnError: true)!;

        return (IAuthSeeder)(Activator.CreateInstance(type, dbContext, hasher, configuration)
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

using SargentNexus.API;
using SargentNexus.Application.Auth;

namespace SargentNexus.API.Tests;

public sealed class StartupSeedingTests
{
    [Fact]
    public async Task SeedAuthAsync_WhenDevelopmentEnvironmentAndDemoRequested_SeedsSiteAdminAndDemoEnvironment()
    {
        var seeder = new RecordingAuthSeeder();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        await StartupSeeding.SeedAuthAsync(
            seeder,
            isDevelopmentEnvironment: true,
            seedDemoRequested: true,
            cancellationToken: cancellationToken);

        Assert.Equal(1, seeder.SeedSiteAdminCallCount);
        Assert.Equal(1, seeder.SeedDevelopmentDemoEnvironmentCallCount);
        Assert.Equal(new[] { "site-admin", "demo" }, seeder.Calls);
        Assert.Equal(new[] { cancellationToken, cancellationToken }, seeder.ReceivedTokens);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SeedAuthAsync_WhenDemoSeedingIsNotEnabled_SeedsSiteAdminOnly(
        bool isDevelopmentEnvironment,
        bool seedDemoRequested)
    {
        var seeder = new RecordingAuthSeeder();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        await StartupSeeding.SeedAuthAsync(
            seeder,
            isDevelopmentEnvironment,
            seedDemoRequested,
            cancellationToken: cancellationToken);

        Assert.Equal(1, seeder.SeedSiteAdminCallCount);
        Assert.Equal(0, seeder.SeedDevelopmentDemoEnvironmentCallCount);
        Assert.Equal(new[] { "site-admin" }, seeder.Calls);
        Assert.Equal(new[] { cancellationToken }, seeder.ReceivedTokens);
    }

    private sealed class RecordingAuthSeeder : IAuthSeeder
    {
        public List<string> Calls { get; } = new();

        public List<CancellationToken> ReceivedTokens { get; } = new();

        public int SeedSiteAdminCallCount { get; private set; }

        public int SeedDevelopmentDemoEnvironmentCallCount { get; private set; }

        public Task SeedSiteAdminAsync(CancellationToken cancellationToken)
        {
            SeedSiteAdminCallCount++;
            Calls.Add("site-admin");
            ReceivedTokens.Add(cancellationToken);
            return Task.CompletedTask;
        }

        public Task SeedDevelopmentDemoEnvironmentAsync(CancellationToken cancellationToken)
        {
            SeedDevelopmentDemoEnvironmentCallCount++;
            Calls.Add("demo");
            ReceivedTokens.Add(cancellationToken);
            return Task.CompletedTask;
        }
    }
}

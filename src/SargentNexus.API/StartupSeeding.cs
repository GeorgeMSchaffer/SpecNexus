using SargentNexus.Application.Auth;

namespace SargentNexus.API;

internal static class StartupSeeding
{
    internal static async Task SeedAuthAsync(
        IAuthSeeder authSeeder,
        bool isDevelopmentEnvironment,
        CancellationToken cancellationToken)
    {
        await authSeeder.SeedSiteAdminAsync(cancellationToken);

        if (isDevelopmentEnvironment)
        {
            await authSeeder.SeedDevelopmentDemoEnvironmentAsync(cancellationToken);
        }
    }
}

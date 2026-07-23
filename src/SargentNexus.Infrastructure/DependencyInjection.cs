using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SargentNexus.Application.Auth;

namespace SargentNexus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SargentNexusDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<InMemoryAccessTokenStore>();
        services.AddScoped<IAuthUserLookup, AuthUserLookup>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
        services.AddScoped<ITemporaryPasswordGenerator, TemporaryPasswordGenerator>();
        services.AddScoped<IAccessTokenIssuer, OpaqueAccessTokenIssuer>();
        services.AddSingleton<IAccessTokenReader>(provider => provider.GetRequiredService<InMemoryAccessTokenStore>());
        services.AddScoped<IAuthAuditWriter, AuthAuditWriter>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IAuthAccountService, AuthAccountService>();
        services.AddScoped<IAuthSeeder, AuthSeeder>();

        return services;
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SargentNexus.Application.Administration;
using SargentNexus.Application.Auth;
using SargentNexus.Application.Workflow;
using SargentNexus.Infrastructure.Administration;
using SargentNexus.Infrastructure.Workflow;

namespace SargentNexus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? hostEnvironment = null)
    {
        var isDevelopment = hostEnvironment?.IsDevelopment() ?? false;
        var useInMemoryDatabase = bool.TryParse(configuration["Database:UseInMemoryDatabase"], out var parsedValue) && parsedValue;

        services.AddDbContext<SargentNexusDbContext>(options =>
        {
            if (useInMemoryDatabase || isDevelopment)
            {
                options.UseInMemoryDatabase("SargentNexus");
                return;
            }

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
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
        services.AddScoped<IOrganizationUserAdministrationStore, OrganizationUserAdministrationStore>();
        services.AddScoped<IOrganizationUserAuditWriter, OrganizationUserAuditWriter>();
        services.AddScoped<IOrganizationUserAdministrationService, OrganizationUserAdministrationService>();
        services.AddScoped<IWorkflowDataAccess, WorkflowDataAccess>();
        services.AddScoped<IWorkflowAuditWriter, WorkflowAuditWriter>();
        services.AddScoped<IWorkflowManagementService, WorkflowManagementService>();

        return services;
    }
}
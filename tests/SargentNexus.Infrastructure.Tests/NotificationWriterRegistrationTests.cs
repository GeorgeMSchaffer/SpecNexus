using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SargentNexus.Application.Workflow;
using SargentNexus.Infrastructure;
using SargentNexus.Infrastructure.Workflow;

namespace SargentNexus.Infrastructure.Tests;

public sealed class NotificationWriterRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersINotificationWriterAsNotificationWriter()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True"
            })
            .Build();

        services.AddInfrastructure(config);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INotificationWriter));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(NotificationWriter), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddInfrastructure_DoesNotRegisterSmtpOrHttpClientForNotifications()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True"
            })
            .Build();

        services.AddInfrastructure(config);

        var hasSmtpClient = services.Any(d => d.ServiceType.FullName != null
            && d.ServiceType.FullName.Contains("SmtpClient", StringComparison.OrdinalIgnoreCase));
        var hasHttpClientFactory = services.Any(d => d.ServiceType.FullName != null
            && d.ServiceType.FullName.Contains("IHttpClientFactory", StringComparison.OrdinalIgnoreCase));

        Assert.False(hasSmtpClient, "SmtpClient must not be registered in the notification path.");
        Assert.False(hasHttpClientFactory, "IHttpClientFactory must not be registered; email delivery is deferred.");
    }
}

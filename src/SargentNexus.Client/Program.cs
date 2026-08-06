using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SargentNexus.Client.Administration;
using SargentNexus.Client;
using SargentNexus.Client.Auth;
using SargentNexus.Client.Workflow;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var resolvedBaseAddress = Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var parsedBaseAddress)
    ? parsedBaseAddress
    : new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped<AuthSessionStorage>();
builder.Services.AddScoped<AuthenticatedSessionHandler>();
builder.Services.AddScoped(sp =>
{
    var authenticatedSessionHandler = sp.GetRequiredService<AuthenticatedSessionHandler>();
    authenticatedSessionHandler.InnerHandler = new HttpClientHandler();

    return new HttpClient(authenticatedSessionHandler)
    {
        BaseAddress = resolvedBaseAddress
    };
});
builder.Services.AddScoped<IAuthSessionService, AuthSessionService>();
builder.Services.AddScoped<IClientErrorMessageService, ClientErrorMessageService>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<AdministrationApiClient>();
builder.Services.AddScoped<WorkflowApiClient>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IClientClock, ClientClock>();

await builder.Build().RunAsync();

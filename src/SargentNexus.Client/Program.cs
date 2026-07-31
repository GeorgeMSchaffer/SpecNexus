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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = resolvedBaseAddress });
builder.Services.AddScoped<IAuthSessionService, AuthSessionService>();
builder.Services.AddScoped<IClientErrorMessageService, ClientErrorMessageService>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<AdministrationApiClient>();
builder.Services.AddScoped<WorkflowApiClient>();

await builder.Build().RunAsync();

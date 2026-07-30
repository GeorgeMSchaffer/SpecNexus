using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace SargentNexus.Client.Auth;

public interface IClientErrorMessageService
{
    bool IsDevelopment { get; }

    string GetDisplayMessage(string? title, string? detail, Exception? exception = null);
}

public sealed class ClientErrorMessageService : IClientErrorMessageService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again.";

    private readonly IWebAssemblyHostEnvironment _environment;

    public ClientErrorMessageService(IWebAssemblyHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool IsDevelopment => _environment.IsDevelopment();

    public string GetDisplayMessage(string? title, string? detail, Exception? exception = null)
    {
        if (!IsDevelopment)
        {
            return GenericErrorMessage;
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(title))
        {
            parts.Add(title);
        }

        if (!string.IsNullOrWhiteSpace(detail))
        {
            parts.Add(detail);
        }

        if (exception is not null && !string.IsNullOrWhiteSpace(exception.Message))
        {
            parts.Add(exception.Message);
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : GenericErrorMessage;
    }
}

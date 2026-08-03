using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace SargentNexus.BrowserTests;

public sealed class SmokeTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private Process? _apiProcess;
    private Process? _clientProcess;

    public async Task InitializeAsync()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright browser install exited with {exitCode}.");
        }

        await StartWebAppAsync();

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
        _page.Console += (_, msg) => Console.WriteLine($"[browser-console:{msg.Type}] {msg.Text}");
        _page.PageError += (_, error) => Console.WriteLine($"[browser-page-error] {error}");
    }

    public async Task DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();

        await StopProcessAsync(_clientProcess);
        await StopProcessAsync(_apiProcess);
        _httpClient.Dispose();
    }

    [Theory]
    [InlineData("siteadmin@sargentnexus.local", "Abc123!Demo")]
    [InlineData("demo.acme.user@sargentnexus.local", "abc123!")]
    public async Task SeededUsersCanSignInAndPersistClientSession(string email, string password)
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await _page.FillAsync("input#email", email);
        await _page.FillAsync("input#password", password);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        try
        {
            await Expect(async () =>
            {
                var accessToken = await _page!.EvaluateAsync<string>("() => window.localStorage.getItem('sn.auth.accessToken') || ''");
                return string.IsNullOrWhiteSpace(accessToken) ? 0 : 1;
            });
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"Current URL after login attempt for {email}: {_page!.Url}");
            Console.WriteLine(await _page.ContentAsync());
            throw;
        }

        var storageDump = await _page.EvaluateAsync("() => Object.fromEntries(Object.entries(window.localStorage))");
        Console.WriteLine($"Auth storage for {email}: {storageDump}");

        var role = await _page.EvaluateAsync<string>("() => window.localStorage.getItem('sn.auth.userRole') || ''");
        var userEmail = await _page.EvaluateAsync<string>("() => window.localStorage.getItem('sn.auth.userEmail') || ''");

        Assert.False(string.IsNullOrWhiteSpace(role));
        Assert.Equal(email, userEmail, ignoreCase: true);
    }

    [Fact]
    public async Task SeededAdminCanSignInCreateBoardAndIdea()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await _page.FillAsync("input#email", "siteadmin@sargentnexus.local");
        await _page.FillAsync("input#password", "Abc123!Demo");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        try
        {
            await Expect(async () => await _page!.Locator("input#currentPassword").CountAsync());
            var storageDump = await _page.EvaluateAsync("() => ({ path: window.location.pathname, localStorage: Object.fromEntries(Object.entries(window.localStorage)) })");
            Console.WriteLine($"Auth storage after login: {storageDump}");
        }
        catch (TimeoutException)
        {
            var currentUrl = _page!.Url;
            Console.WriteLine($"Current URL after login: {currentUrl}");
            Console.WriteLine(await _page.ContentAsync());
            throw;
        }

        await _page.FillAsync("input#currentPassword", "Abc123!Demo");
        await _page.FillAsync("input#newPassword", "Abc123!Demo2");
        await _page.FillAsync("input#confirmNewPassword", "Abc123!Demo2");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Change password" }).ClickAsync();

        await Expect(async () =>
        {
            var path = await _page!.EvaluateAsync("() => window.location.pathname");
            return string.Equals(path.ToString(), "/workflow", StringComparison.OrdinalIgnoreCase) || string.Equals(path.ToString(), "/", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        });

        if (!string.Equals((await _page!.EvaluateAsync("() => window.location.pathname")).ToString(), "/workflow", StringComparison.OrdinalIgnoreCase))
        {
            await _page.GotoAsync(baseUrl + "/workflow", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }
        try
        {
            await Expect(async () => await _page!.Locator("text=Workflow").CountAsync());
            var buttonTexts = await _page.GetByRole(AriaRole.Button).AllTextContentsAsync();
            Console.WriteLine("Workflow button texts: " + string.Join(" | ", buttonTexts));
            var workflowHtml = await _page.ContentAsync();
            var workflowDebugPath = Path.Combine(AppContext.BaseDirectory, "workflow-debug.html");
            await File.WriteAllTextAsync(workflowDebugPath, workflowHtml);
            Console.WriteLine($"Workflow HTML written to {workflowDebugPath}");
            var storageDump = await _page.EvaluateAsync("() => Object.fromEntries(Object.entries(window.localStorage))");
            var storageDebugPath = Path.Combine(AppContext.BaseDirectory, "workflow-storage.json");
            await File.WriteAllTextAsync(storageDebugPath, System.Text.Json.JsonSerializer.Serialize(storageDump));
            Console.WriteLine($"Workflow storage written to {storageDebugPath}");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Create board" }).ClickAsync();
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"Workflow URL after navigation: {_page!.Url}");
            var workflowHtml = await _page.ContentAsync();
            var workflowDebugPath = Path.Combine(AppContext.BaseDirectory, "workflow-debug.html");
            await File.WriteAllTextAsync(workflowDebugPath, workflowHtml);
            Console.WriteLine($"Workflow HTML written to {workflowDebugPath}");
            var storageDump = await _page.EvaluateAsync("() => Object.fromEntries(Object.entries(window.localStorage))");
            var storageDebugPath = Path.Combine(AppContext.BaseDirectory, "workflow-storage.json");
            await File.WriteAllTextAsync(storageDebugPath, System.Text.Json.JsonSerializer.Serialize(storageDump));
            Console.WriteLine($"Workflow storage written to {storageDebugPath}");
            throw;
        }

        var boardName = $"Smoke Board {DateTime.UtcNow:yyyyMMddHHmmss}";
        await _page.FillAsync("input[name='boardName']", boardName);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Save board" }).ClickAsync();

        await Expect(async () => await _page!.Locator($"text={boardName}").CountAsync());

        await _page.GetByRole(AriaRole.Button, new() { Name = "Create idea" }).ClickAsync();
        await _page.FillAsync("input[name='title']", "Smoke idea");
        await _page.FillAsync("textarea[name='description']", "Created by browser smoke test.");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        await Expect(async () => await _page!.Locator("text=Smoke idea").CountAsync());
    }

    [Fact]
    public async Task LogoutClearsAuthTokenFromLocalStorage()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await _page.FillAsync("input#email", "demo.acme.user@sargentnexus.local");
        await _page.FillAsync("input#password", "abc123!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await Expect(async () =>
        {
            var accessToken = await _page!.EvaluateAsync<string>("() => window.localStorage.getItem('sn.auth.accessToken') || ''");
            return string.IsNullOrWhiteSpace(accessToken) ? 0 : 1;
        });

        await _page.GotoAsync(baseUrl + "/logout", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        try
        {
            await Expect(async () =>
            {
                var accessToken = await _page!.EvaluateAsync<string>("() => window.localStorage.getItem('sn.auth.accessToken') || ''");
                var path = (await _page.EvaluateAsync("() => window.location.pathname")).ToString();
                return string.IsNullOrWhiteSpace(accessToken) && string.Equals(path, "/login", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            });
        }
        catch (TimeoutException)
        {
            var diag = await _page.EvaluateAsync("() => ({ path: window.location.pathname, localStorage: Object.fromEntries(Object.entries(window.localStorage)) })");
            Console.WriteLine($"Post-logout state: {diag}");
            Console.WriteLine(await _page.ContentAsync());
            throw;
        }

        var remainingAuthKeys = await _page.EvaluateAsync<string[]>("() => Object.keys(window.localStorage).filter(k => k.startsWith('sn.auth.'))");
        Assert.Empty(remainingAuthKeys);
    }

    private async Task StartWebAppAsync()
    {
        var root = FindRepositoryRoot();
        await StartProcessIfNeededAsync(
            "API",
            "http://127.0.0.1:5027/api/v1/health",
            "dotnet",
            new[]
            {
                "run",
                "--project",
                Path.Combine(root, "src", "SargentNexus.API"),
                "--no-launch-profile",
                "--urls",
                "http://127.0.0.1:5027"
            },
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["DOTNET_ENVIRONMENT"] = "Development"
            },
            process => _apiProcess = process);

        await StartProcessIfNeededAsync(
            "Client",
            "http://127.0.0.1:5237/",
            "dotnet",
            new[]
            {
                "run",
                "--project",
                Path.Combine(root, "src", "SargentNexus.Client"),
                "--no-launch-profile",
                "--urls",
                "http://127.0.0.1:5237"
            },
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["DOTNET_ENVIRONMENT"] = "Development",
                ["ApiBaseUrl"] = "http://127.0.0.1:5027"
            },
            process => _clientProcess = process);
    }

    private async Task StartProcessIfNeededAsync(string name, string healthUrl, string fileName, IReadOnlyList<string> args, IReadOnlyDictionary<string, string?> environment, Action<Process> assignProcess)
    {
        try
        {
            using var response = await _httpClient.GetAsync(healthUrl);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
        }
        catch
        {
            // The process is not available yet; the next block will start it.
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = FindRepositoryRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        foreach (var pair in environment)
        {
            process.StartInfo.Environment[pair.Key] = pair.Value;
        }

        process.OutputDataReceived += (_, eventArgs) =>
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            {
                Console.WriteLine($"[{name}] {eventArgs.Data}");
            }
        };

        process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            {
                Console.Error.WriteLine($"[{name}] {eventArgs.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        assignProcess(process);

        var timeout = DateTime.UtcNow.AddSeconds(180);
        while (DateTime.UtcNow < timeout)
        {
            try
            {
                using var response = await _httpClient.GetAsync(healthUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Continue waiting until the process becomes reachable.
            }

            if (process.HasExited)
            {
                throw new InvalidOperationException($"The {name} process exited before it became healthy.");
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"Timed out waiting for the {name} process to become healthy.");
    }

    private async Task StopProcessAsync(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        catch
        {
            // Ignore shutdown problems for test cleanup.
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "SargentNexus.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }

    private static async Task Expect(Func<Task<int>> assertion)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (await assertion() > 0)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Timed out waiting for the expected UI state.");
    }
}

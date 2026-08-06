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
        var apiBaseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_API_BASE_URL");
        if (!string.IsNullOrWhiteSpace(apiBaseUrl)
            && !string.Equals(apiBaseUrl, "http://127.0.0.1:5027", StringComparison.OrdinalIgnoreCase))
        {
            await _page.RouteAsync("http://127.0.0.1:5027/**", route =>
                route.ContinueAsync(new RouteContinueOptions
                {
                    Url = route.Request.Url.Replace("http://127.0.0.1:5027", apiBaseUrl, StringComparison.OrdinalIgnoreCase)
                }));
        }
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
        Assert.Equal("/settings", await _page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).GetAttributeAsync("href"));
        Assert.Equal("/logout", await _page.GetByRole(AriaRole.Link, new() { Name = "Sign out", Exact = true }).GetAttributeAsync("href"));

        if (string.Equals(email, "demo.acme.user@sargentnexus.local", StringComparison.OrdinalIgnoreCase))
        {
            await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            Assert.Equal(0, await _page.GetByText("Password update required", new() { Exact = true }).CountAsync());
            Assert.Equal(0, await _page.GetByRole(AriaRole.Link, new() { Name = "Workflows", Exact = true }).CountAsync());

            await _page.GotoAsync(baseUrl + "/change-password", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/settings/profile", StringComparison.OrdinalIgnoreCase) ? 1 : 0));

            foreach (var legacyPath in new[] { "/board", "/workflow", "/workflows" })
            {
                await _page.EvaluateAsync("path => Blazor.navigateTo(path)", legacyPath);
                try
                {
                    await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/boards", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
                }
                catch (TimeoutException ex)
                {
                    throw new TimeoutException($"Legacy route '{legacyPath}' remained at '{new Uri(_page.Url).AbsolutePath}' instead of redirecting to '/boards'.", ex);
                }
            }
        }
    }

    [Fact]
    public async Task AnonymousProtectedRouteRedirectsToLogin()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";

        await _page!.GotoAsync(baseUrl + "/boards", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/login", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
    }

    [Fact]
    public async Task StoredInvalidToken_IsClearedAndRedirectsToLogin()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.EvaluateAsync("""
            () => {
                localStorage.setItem('sn.auth.accessToken', 'invalid-token');
                localStorage.setItem('sn.auth.requiresPasswordChange', 'false');
                localStorage.setItem('sn.auth.userEmail', 'cached@sargentnexus.local');
                localStorage.setItem('sn.auth.userRole', 'User');
                localStorage.setItem('sn.auth.userId', '11111111-1111-1111-1111-111111111111');
                localStorage.setItem('sn.auth.organizationId', '22222222-2222-2222-2222-222222222222');
                localStorage.setItem('sn.auth.firstName', 'Cached');
                localStorage.setItem('sn.auth.lastName', 'User');
                localStorage.setItem('sn.auth.status', 'Active');
            }
            """);

        await _page.GotoAsync(baseUrl + "/boards", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/login", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
        var authKeyCount = await _page.EvaluateAsync<int>("() => Object.keys(localStorage).filter(key => key.startsWith('sn.auth.')).length");
        Assert.Equal(0, authKeyCount);
    }

    [Fact]
    public async Task ProtectedRequest_WhenTokenValidationAlsoReturnsUnauthorized_ClearsSession()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await SignInAsync(baseUrl, "demo.acme.user@sargentnexus.local", "abc123!");

        await _page!.RouteAsync("**/api/v1/auth/me", route => route.FulfillAsync(new RouteFulfillOptions
        {
            Status = 401,
            ContentType = "application/problem+json",
            Body = "{\"title\":\"Authentication required.\",\"status\":401}"
        }));
        await _page.RouteAsync("**/api/v1/organizations/*/boards", route => route.FulfillAsync(new RouteFulfillOptions
        {
            Status = 401,
            ContentType = "application/problem+json",
            Body = "{\"title\":\"Authentication required.\",\"status\":401}"
        }));

        await _page.EvaluateAsync("Blazor.navigateTo('/boards')");

        await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/login", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
        Assert.Equal(string.Empty, await _page.EvaluateAsync<string>("() => localStorage.getItem('sn.auth.accessToken') || ''"));
    }

    [Fact]
    public async Task IncorrectCurrentPassword_DoesNotClearValidSession()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await SignInAsync(baseUrl, "demo.acme.user@sargentnexus.local", "abc123!");
        await _page!.EvaluateAsync("Blazor.navigateTo('/settings/profile')");
        await Expect(async () => await _page.Locator("input#profileCurrentPassword").CountAsync());

        await _page.FillAsync("input#profileCurrentPassword", "WrongPassword1!");
        await _page.FillAsync("input#profileNewPassword", "Abc123!Demo2");
        await _page.FillAsync("input#profileConfirmNewPassword", "Abc123!Demo2");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Change password" }).ClickAsync();

        await Expect(async () => await _page.GetByText("Invalid current password.", new() { Exact = true }).CountAsync());
        Assert.Equal("/settings/profile", new Uri(_page.Url).AbsolutePath);
        Assert.False(string.IsNullOrWhiteSpace(
            await _page.EvaluateAsync<string>("() => localStorage.getItem('sn.auth.accessToken') || ''")));
    }

    [Fact]
    public async Task SeededAdminCanSignInCreateBoardAndIdea()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await _page.FillAsync("input#email", "demo.acme.orgadmin@sargentnexus.local");
        await _page.FillAsync("input#password", "abc123!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        await Expect(async () =>
        {
            var path = await _page!.EvaluateAsync("() => window.location.pathname");
            return string.Equals(path.ToString(), "/", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        });
        Assert.Equal("/settings", await _page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).GetAttributeAsync("href"));
        Assert.Equal("/logout", await _page.GetByRole(AriaRole.Link, new() { Name = "Sign out", Exact = true }).GetAttributeAsync("href"));

        await _page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(() => Task.FromResult(string.Equals(new Uri(_page.Url).AbsolutePath, "/", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
        Assert.False(string.IsNullOrWhiteSpace(
            await _page.EvaluateAsync<string>("() => localStorage.getItem('sn.auth.accessToken') || ''")));

        await _page.GotoAsync(baseUrl + "/settings/boards", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        try
        {
            await Expect(async () => await _page!.GetByRole(AriaRole.Heading, new() { Name = "Boards" }).CountAsync());
            var buttonTexts = await _page.GetByRole(AriaRole.Button).AllTextContentsAsync();
            Console.WriteLine("Board settings button texts: " + string.Join(" | ", buttonTexts));
            var storageDump = await _page.EvaluateAsync("() => Object.fromEntries(Object.entries(window.localStorage))");
            var storageDebugPath = Path.Combine(AppContext.BaseDirectory, "board-storage.json");
            await File.WriteAllTextAsync(storageDebugPath, System.Text.Json.JsonSerializer.Serialize(storageDump));
            Console.WriteLine($"Board storage written to {storageDebugPath}");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add new board" }).ClickAsync();
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"Board settings URL after navigation: {_page!.Url}");
            var boardHtml = await _page.ContentAsync();
            var boardDebugPath = Path.Combine(AppContext.BaseDirectory, "board-debug.html");
            await File.WriteAllTextAsync(boardDebugPath, boardHtml);
            Console.WriteLine($"Board HTML written to {boardDebugPath}");
            var storageDump = await _page.EvaluateAsync("() => Object.fromEntries(Object.entries(window.localStorage))");
            var storageDebugPath = Path.Combine(AppContext.BaseDirectory, "board-storage.json");
            await File.WriteAllTextAsync(storageDebugPath, System.Text.Json.JsonSerializer.Serialize(storageDump));
            Console.WriteLine($"Board storage written to {storageDebugPath}");
            throw;
        }

        var boardName = $"Smoke Board {DateTime.UtcNow:yyyyMMddHHmmss}";
        await _page.Locator("form input.form-control").FillAsync(boardName);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

        try
        {
            await Expect(async () => await _page!.GetByText(boardName, new() { Exact = true }).CountAsync());
        }
        catch (TimeoutException ex)
        {
            var alerts = await _page.Locator("[role='alert'], .alert").AllTextContentsAsync();
            throw new TimeoutException($"Board '{boardName}' was not shown after save at '{new Uri(_page.Url).AbsolutePath}'. Alerts: {string.Join(" | ", alerts)}", ex);
        }

        await _page.EvaluateAsync("Blazor.navigateTo('/ideas')");
        await _page.Locator("select#newIdeaBoard").SelectOptionAsync(new SelectOptionValue { Label = boardName });
        await _page.GetByRole(AriaRole.Button, new() { Name = "Add new idea" }).ClickAsync();
        await Expect(async () => await _page!.Locator(".workflow-board__create-form").CountAsync());
        await _page.Locator(".workflow-board__create-form input.form-control").First.FillAsync("Smoke idea");
        await _page.Locator(".workflow-board__create-form textarea").FillAsync("Created by browser smoke test.");
        await _page.Locator(".workflow-board__create-form").GetByRole(AriaRole.Button, new() { Name = "Create idea" }).ClickAsync();

        await Expect(async () => await _page!.Locator("text=Smoke idea").CountAsync());
    }

    private async Task SignInAsync(string baseUrl, string email, string password)
    {
        await _page!.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.FillAsync("input#email", email);
        await _page.FillAsync("input#password", password);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
        await Expect(async () =>
        {
            var accessToken = await _page.EvaluateAsync<string>("() => localStorage.getItem('sn.auth.accessToken') || ''");
            return string.IsNullOrWhiteSpace(accessToken) ? 0 : 1;
        });
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
            await _page.WaitForURLAsync("**/login");
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
        Assert.Equal(0, await _page.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true }).CountAsync());
        Assert.Equal(0, await _page.GetByRole(AriaRole.Link, new() { Name = "Sign out", Exact = true }).CountAsync());
    }

    private async Task StartWebAppAsync()
    {
        var root = FindRepositoryRoot();
        var apiBaseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_API_BASE_URL") ?? "http://127.0.0.1:5027";
        var clientBaseUrl = Environment.GetEnvironmentVariable("SARGENTNEXUS_BASE_URL") ?? "http://127.0.0.1:5237";
        await StartProcessIfNeededAsync(
            "API",
            $"{apiBaseUrl}/api/v1/health",
            "dotnet",
            new[]
            {
                "run",
                "--project",
                Path.Combine(root, "src", "SargentNexus.API"),
                "--configuration",
                "Release",
                "--no-build",
                "--no-launch-profile",
                "--urls",
                apiBaseUrl,
                "--",
                "--seed-demo"
            },
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["DOTNET_ENVIRONMENT"] = "Development",
                ["Cors__AllowedOrigin"] = clientBaseUrl
            },
            process => _apiProcess = process);

        await StartProcessIfNeededAsync(
            "Client",
            $"{clientBaseUrl}/",
            "dotnet",
            new[]
            {
                "run",
                "--project",
                Path.Combine(root, "src", "SargentNexus.Client"),
                "--configuration",
                "Release",
                "--no-build",
                "--no-launch-profile",
                "--urls",
                clientBaseUrl
            },
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["DOTNET_ENVIRONMENT"] = "Development",
                ["ApiBaseUrl"] = apiBaseUrl
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

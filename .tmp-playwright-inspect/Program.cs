using System.Diagnostics;
using Microsoft.Playwright;

var pw = await Playwright.CreateAsync();
var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
var page = await browser.NewPageAsync();
page.Console += (_, msg) => Console.WriteLine($"[browser-console:{msg.Type}] {msg.Text}");
page.PageError += (_, err) => Console.WriteLine($"[browser-page-error] {err}");

var baseUrl = "http://127.0.0.1:5237";
await page.GotoAsync(baseUrl + "/login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
await page.FillAsync("input#email", "siteadmin@sargentnexus.local");
await page.FillAsync("input#password", "Abc123!Demo");
await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

await page.WaitForTimeoutAsync(2000);
Console.WriteLine("URL after login click: " + page.Url);
Console.WriteLine("Content after login click:\n" + await page.ContentAsync());

if (await page.Locator("input#currentPassword").CountAsync() > 0)
{
    await page.FillAsync("input#currentPassword", "Abc123!Demo");
    await page.FillAsync("input#newPassword", "Abc123!Demo2");
    await page.FillAsync("input#confirmNewPassword", "Abc123!Demo2");
    await page.GetByRole(AriaRole.Button, new() { Name = "Change password" }).ClickAsync();
    await page.WaitForTimeoutAsync(2000);
    Console.WriteLine("URL after change password: " + page.Url);
    Console.WriteLine("Content after change password:\n" + await page.ContentAsync());
}

await page.GotoAsync(baseUrl + "/workflow", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
await page.WaitForTimeoutAsync(4000);
Console.WriteLine("URL at workflow: " + page.Url);
Console.WriteLine("Content at workflow:\n" + await page.ContentAsync());
Console.WriteLine("Button texts: " + string.Join(" | ", await page.GetByRole(AriaRole.Button).AllTextContentsAsync()));
Console.WriteLine("Render count Create board: " + await page.GetByRole(AriaRole.Button, new() { Name = "Create board" }).CountAsync());

await browser.CloseAsync();
pw.Dispose();

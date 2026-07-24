# Setup and Configuration

Use `Microsoft.FluentUI.AspNetCore.Components` for the core Blazor components.

## Basic setup

1. Add the NuGet package.
2. Register Fluent UI services in `Program.cs` with `AddFluentUIComponents()`.
3. Add the required providers to the root layout.

## Common checks

- If a toast, dialog, or tooltip does nothing, confirm the matching provider exists.
- If icons are missing, verify the separate icons package is installed.
- If a component depends on design tokens, make sure the code runs after the first render.

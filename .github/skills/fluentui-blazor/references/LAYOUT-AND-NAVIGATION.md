# Layout and Navigation

Fluent UI Blazor layouts usually place the service providers in `MainLayout.razor` so
the rest of the app can use dialogs, toasts, tooltips, and message bars.

## Layout guidance

- Keep the providers near the top level of the app shell.
- Ensure the layout is rendered for the pages that need Fluent services.
- Use `FluentNavMenu` and related navigation components in the app shell, not inside
  isolated feature views unless that is intentional.

## Troubleshooting

- Missing UI from service components often means the provider is absent from the active layout.
- If a component works on one page but not another, check whether both pages share the same layout.

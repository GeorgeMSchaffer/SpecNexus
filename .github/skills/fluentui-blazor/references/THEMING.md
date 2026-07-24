# Theming

Fluent UI theme and design token APIs rely on browser-side interop, so they should be
configured after the component has rendered.

## Guidance

- Use `FluentDesignTheme` to define the active theme.
- Set or update tokens in `OnAfterRenderAsync` when the UI depends on browser state.
- Verify storage or persistence behavior if the theme should survive reloads.

## Troubleshooting

- If theme changes do not apply, check that the component has rendered and that JS interop is available.
- If colors look inconsistent, confirm the app is using the expected Fluent theme and not a competing custom stylesheet.
---
name: fluentui-blazor
description: >
  Guide for using the Microsoft Fluent UI Blazor component library
  (Microsoft.FluentUI.AspNetCore.Components NuGet package) in Blazor applications.
  Use this when the user is building a Blazor app with Fluent UI components,
  setting up the library, using FluentUI components like FluentButton, FluentDataGrid,
  FluentDialog, FluentToast, FluentNavMenu, FluentTextField, FluentSelect,
  FluentAutocomplete, FluentDesignTheme, or any component prefixed with "Fluent".
  Also use when troubleshooting missing providers, JS interop issues, or theming.
---

# Fluent UI Blazor - Consumer Usage Guide

This skill teaches how to correctly use the Microsoft.FluentUI.AspNetCore.Components
package in Blazor applications.

## Critical Rules

### 1. Do not add manual script or link tags

The library loads its assets through Blazor static web assets and JS initializers.
Do not tell users to add manual `<script>` or `<link>` tags for the core library.

### 2. Providers are mandatory for service-based components

These provider components must be added to the root layout, such as `MainLayout.razor`,
for their corresponding services to work.

```razor
<FluentToastProvider />
<FluentDialogProvider />
<FluentMessageBarProvider />
<FluentTooltipProvider />
<FluentKeyCodeProvider />
```

If a provider is missing, service calls can fail silently with no UI.

### 3. Register services in Program.cs

```csharp
builder.Services.AddFluentUIComponents();

// Or with configuration:
builder.Services.AddFluentUIComponents(options =>
{
    options.UseTooltipServiceProvider = true;
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
```

Service lifetime guidance:
- `Scoped` for Blazor Server and interactive hosting, which is the default
- `Singleton` for standalone Blazor WebAssembly
- `Transient` is not supported and throws `NotSupportedException`

### 4. Icons require a separate package

```bash
dotnet add package Microsoft.FluentUI.AspNetCore.Components.Icons
```

Use a typed alias:

```razor
@using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons

<FluentIcon Value="@(Icons.Regular.Size24.Save)" />
<FluentIcon Value="@(Icons.Filled.Size20.Delete)" Color="@Color.Error" />
```

Use the `Icons.[Variant].[Size].[Name]` pattern. Do not use string-based icon names.

### 5. List-style components use option binding, not InputSelect binding

`FluentSelect<TOption>`, `FluentCombobox<TOption>`, `FluentListbox<TOption>`, and
`FluentAutocomplete<TOption>` use:
- `Items`
- `OptionText`
- `OptionValue`
- `SelectedOption` / `SelectedOptionChanged`
- `SelectedOptions` / `SelectedOptionsChanged`

```razor
<FluentSelect Items="@countries"
              OptionText="@(c => c.Name)"
              OptionValue="@(c => c.Code)"
              @bind-SelectedOption="@selectedCountry"
              Label="Country" />
```

Do not use the `InputSelect` pattern with `<option>` children.

### 6. FluentAutocomplete specifics

- Use `ValueText` rather than `Value`
- `OnOptionsSearch` is required for filtering options
- `Multiple` defaults to `true`

```razor
<FluentAutocomplete TOption="Person"
                    OnOptionsSearch="@OnSearch"
                    OptionText="@(p => p.FullName)"
                    @bind-SelectedOptions="@selectedPeople"
                    Label="Search people" />

@code {
    private void OnSearch(OptionsSearchEventArgs<Person> args)
    {
        args.Items = allPeople.Where(p =>
            p.FullName.Contains(args.Text, StringComparison.OrdinalIgnoreCase));
    }
}
```

### 7. Dialogs use the service pattern

Do not toggle the visibility of `<FluentDialog>` tags directly. Use a dialog content
component plus `IDialogService`.

```csharp
public partial class EditPersonDialog : IDialogContentComponent<Person>
{
    [Parameter] public Person Content { get; set; } = default!;

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    private async Task SaveAsync()
    {
        await Dialog.CloseAsync(Content);
    }

    private async Task CancelAsync()
    {
        await Dialog.CancelAsync();
    }
}
```

```csharp
[Inject] private IDialogService DialogService { get; set; } = default!;

private async Task ShowEditDialog()
{
    var dialog = await DialogService.ShowDialogAsync<EditPersonDialog, Person>(
        person,
        new DialogParameters
        {
            Title = "Edit Person",
            PrimaryAction = "Save",
            SecondaryAction = "Cancel",
            Width = "500px",
            PreventDismissOnOverlayClick = true,
        });

    var result = await dialog.Result;
    if (!result.Cancelled)
    {
        var updatedPerson = result.Data as Person;
    }
}
```

For convenience dialogs, use `ShowConfirmationAsync`, `ShowSuccessAsync`, and
`ShowErrorAsync`.

### 8. Toast notifications

```csharp
[Inject] private IToastService ToastService { get; set; } = default!;

ToastService.ShowSuccess("Item saved successfully");
ToastService.ShowError("Failed to save");
ToastService.ShowWarning("Check your input");
ToastService.ShowInfo("New update available");
```

`FluentToastProvider` supports `Position`, `Timeout`, and `MaxToastCount`.

### 9. Design tokens and themes work after render

Design tokens depend on JS interop. Do not set them in `OnInitialized`; use
`OnAfterRenderAsync`.

```razor
<FluentDesignTheme Mode="DesignThemeModes.System"
                   OfficeColor="OfficeColor.Teams"
                   StorageName="mytheme" />
```

### 10. FluentEditForm versus EditForm

Use `FluentEditForm` only inside `FluentWizard` steps when you need per-step validation.
For regular forms, use `EditForm` with Fluent inputs and Fluent validation components.

```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <FluentTextField @bind-Value="@model.Name" Label="Name" Required />
    <FluentSelect Items="@options"
                  OptionText="@(o => o.Label)"
                  @bind-SelectedOption="@model.Category"
                  Label="Category" />
    <FluentValidationSummary />
    <FluentButton Type="ButtonType.Submit" Appearance="Appearance.Accent">Save</FluentButton>
</EditForm>
```

## Reference Files

See the companion notes for focused guidance:

- [Setup and configuration](references/SETUP.md)
- [Layout and navigation](references/LAYOUT-AND-NAVIGATION.md)
- [Data grid](references/DATAGRID.md)
- [Theming](references/THEMING.md)
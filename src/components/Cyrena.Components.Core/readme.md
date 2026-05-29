## Overview

`Cyrena.Components.Core` is the Blazor component library providing UI contracts, base classes, shared components, and extension methods for building UI components that integrate with Cyréna's kernel-scoped services. Extensions that add UI elements (toolbars, settings pages, shortcuts, docked panels) must reference this package.

**Version:** 0.6.0
**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Contracts`, `Cyrena.Models`, `Cyrena.Options`, `Cyrena.Extensions`, `Cyrena.Attributes`, `Cyrena.Components.Shared`

---

## Contracts

### `IShortcut`
Defines a keyboard shortcut or quick action that appears in the UI.

```csharp
public interface IShortcut
{
    string Title { get; }
    string Description { get; }
    string Icon { get; }
    string Color { get; }
    string Category { get; }
    string[] Tags { get; }
    Task OnClick();
}
```

### `IToolbarComponent`
Defines a component that renders in the chat toolbar.

```csharp
public interface IToolbarComponent
{
    Type Component { get; }
    ToolbarAlignment Alignment { get; }
}

public enum ToolbarAlignment
{
    Start, End
}
```

- `Component`: The `Type` of the Blazor component to render.
- `Alignment`: `Start` (left side) or `End` (right side) of the toolbar.

### `IDockingService`
Manages docked panel components in the UI.

```csharp
public interface IDockingService
{
    public record DockRequest(Type Component, string Title, Action OnClose);
    IDisposable OnDockRequest(Action<DockRequest> callback);
    void Dock<TKernelComponent>(string title, Action onClose)
        where TKernelComponent : KernelComponentBase;
}
```

- `DockRequest`: Record containing component type, title, and close callback.
- `OnDockRequest`: Subscribe to dock request events.
- `Dock<TKernelComponent>`: Docks a `KernelComponentBase`-derived component.

### `IViewStartProvider`
Provides optional `ViewStart` entries for user-configurable starting views.

```csharp
public interface IViewStartProvider
{
    IEnumerable<ViewStart> Provide();
}
```

---

## Attributes

### `KernelInjectAttribute`
Indicates that the associated property should have a value injected from the `KernelComponentBase.Kernel` service provider.

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class KernelInjectAttribute : Attribute
{
    public object? Key { get; init; }
}
```

- `Key`: Optional keyed service key for `GetKeyedService` resolution.

**Usage:**
```csharp
public class MyToolbarComponent : KernelComponentBase
{
    [KernelInject]
    public IChatMessageService ChatService { get; set; } = default!;
    
    [KernelInject(Key = "my-key")]
    public IMyService MyService { get; set; } = default!;
}
```

---

## Models

### `KernelComponentBase`
Base class for Blazor components that need access to the current `Kernel` and its services via `[KernelInject]`.

```csharp
public abstract class KernelComponentBase : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Kernel Kernel { get; set; } = default!;
}
```

Components extending this class receive the active kernel instance as a parameter. When `Kernel` is set, all properties marked with `[KernelInject]` are automatically resolved from `Kernel.Services` via reflection.

### `IWindowHandle`
Represents a handle to an opened browser window for lifecycle management.

```csharp
public interface IWindowHandle : IDisposable
{
    event EventHandler<EventArgs>? Closing;
    bool Disposed { get; }
    void Close();
}
```

### `ViewStart`
Information for a configurable starting view.

```csharp
public sealed class ViewStart
{
    public string Href { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}
```

---

## Options

### `ComponentOptions`
Configuration for UI components, particularly settings pages.

```csharp
public class ComponentOptions
{
    internal List<ComponentMetaData> SettingsComponents { get; set; }
    public ComponentMetaData[] GetSettingsComponents();
}

public record ComponentMetaData(Type Component, string? Section, int Order);
```

### `ComponentOptionsExtensions`
Extension methods for registering settings components:

```csharp
public static class ComponentOptionsExtensions
{
    [Obsolete("Use new section mapping API")]
    public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
        where TComponent : ComponentBase;

    public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section);

    public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section, int order);
}
```

- All overloads prevent duplicate registration by checking if the component type already exists.
- The no-parameter overload is **obsolete** — use section overloads instead.

### `CodeLanguages`
Maps file extensions to syntax highlighting language identifiers.

```csharp
public class CodeLanguages
{
    public string GetFileLanguage(string extension);
}
```

**Supported mappings:**
- `.c`, `.h` → `c`
- `.cpp`, `.hpp`, `.ino` → `cpp`
- `.cs` → `csharp`
- `.razor` → `html`
- `.css` → `css`
- `.js` → `javascript`
- `.md` → `markdown`
- `.csproj`, `.xml` → `xml`
- `.json` → `json`
- Default → `plaintext`

---

## Extension Methods

### `CyrenaBuilderExtensions`

```csharp
public static class CyrenaBuilderExtensions
{
    [Obsolete("Use new section mapping API")]
    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section, int order)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddShortcut<TShortcut>(this CyrenaBuilder builder)
        where TShortcut : class, IShortcut;
}
```

### `CyrenaKernelBuilderExtensions`

```csharp
public static class CyrenaKernelBuilderExtensions
{
    public static void AddToolbarComponent<TComponent>(this CyrenaKernelBuilder builder, ToolbarAlignment alignment)
        where TComponent : KernelComponentBase;

    [Obsolete]
    public static void AddToolbarComponent<TComponent>(this IKernelBuilder builder, ToolbarAlignment alignment)
        where TComponent : KernelComponentBase;
}
```

### `ComponentBaseExtensions`

```csharp
public static class ComponentBaseExtensions
{
    public static RenderFragment Render(this ComponentBase cmp, Type type);
    public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters);
}
```

### `DialogServiceExtensions`

```csharp
public static class DialogServiceExtensions
{
    public static async Task<bool> ShowDialogAsync<TComponent>(
        this IDialogService dialog, 
        string title, 
        DialogParameters parameters, 
        MaxWidth maxWidth = MaxWidth.Medium)
        where TComponent : ComponentBase;
}
```

Helper method that shows a MudBlazor dialog and returns `true` if the user confirmed (not canceled), `false` otherwise. Sets `FullWidth = true` automatically.

---

## Shared Blazor Components

### `CodeInput.razor`
Monaco code editor wrapper (BlazorMonaco) with syntax highlighting, dark theme, and two-way value binding.

**Parameters:**
- `Value` / `ValueChanged` — Two-way bound editor content
- `Language` — Monaco language mode (default: `"plaintext"`)

### `ConnectionSelector.razor`
MudBlazor dropdown (`MudSelect`) of available AI connections from all registered `IConnectionProvider` services.

**Parameters:**
- `Value` / `ValueChanged` — Two-way bound selected connection ID
- `Label` — Dropdown label (default: `"AI Connection"`)
- `Required` — Whether selection is required (default: `true`)

**Behavior:** Populates connections on first render. Displays `Name (Source)` per option.

### `PluginSelector.razor`
MudBlazor checkbox list (`MudCheckBox`) for activating/deactivating `IAssistantPlugin` instances filtered by the current chat's assistant mode.

**Parameters:**
- `Chat` (`ChatConfiguration`, required) — Chat whose `PluginIds` will be updated

**Behavior:**
- Filters plugins by mode compatibility
- Required plugins are always selected and disabled
- If `Chat.PluginIds` is empty, all plugins are selected by default
- Updates `Chat.PluginIds` on every selection change

### `HistoryConfiguration.razor`
MudBlazor dropdown (`MudSelect`) for configuring chat history inclusion mode.

**Parameters:**
- `Model` (`ChatConfiguration`, required) — Chat whose `HistoryInclusion` will be updated

**Options:** All, Last 2 Iterations, Last 10 Iterations, Instruct (no history)

---

## Usage for Extension Developers

Reference `Cyrena.Components.Core` to:
1. Implement `IShortcut` for quick actions
2. Implement `IToolbarComponent` for toolbar buttons
3. Extend `KernelComponentBase` for kernel-aware UI components
4. Use `IDockingService` for docked panels
5. Use `[KernelInject]` for automatic service injection from kernel scope
6. Register settings components via `AddSettingsComponent`
7. Register toolbar components via `AddToolbarComponent`
8. Implement `IViewStartProvider` for custom starting views
9. Use `DialogServiceExtensions.ShowDialogAsync<TComponent>()` for simple confirmation dialogs

**Example - Toolbar Component:**
```csharp
public class MyToolbarComponent : KernelComponentBase 
{ 
    [KernelInject]
    public IChatMessageService ChatService { get; set; } = default!;
}

// In IAssistantPlugin.LoadAsync:
builder.AddToolbarComponent<MyToolbarComponent>(ToolbarAlignment.End);
```

**Example - Shortcut:**
```csharp
public class MyShortcut : IShortcut
{
    public string Title => "My Action";
    public string Description => "Does something";
    public string Icon => Icons.Material.Filled.Star;
    public string Color => "primary";
    public string Category => "My Category";
    public string[] Tags => ["my"];
    public Task OnClick() { ... }
}

// In extension BuildExtension:
builder.AddShortcut<MyShortcut>();
```

**Example - Settings Component:**
```csharp
builder.AddSettingsComponent<MySettingsComponent>("General", 1);
```

**Example - Dialog:**
```csharp
var parameters = new DialogParameters<MyDialogForm>
{
    { x => x.Model, model }
};
var confirmed = await _dialog.ShowDialogAsync<MyDialogForm>("Title", parameters);
if (confirmed)
{
    // User clicked Submit
}
```
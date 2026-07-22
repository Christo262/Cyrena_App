## Overview

`Cyrena.Components.Core` is the Blazor component library providing UI contracts, base classes, shared components, and extension methods for building UI components that integrate with Cyréna's kernel-scoped services. Extensions that add UI elements (toolbars, settings pages, shortcuts, docked panels) must reference this package.

**Version:** 0.6.0
**Target Framework:** .NET 10.0

**Namespaces:** `Cyrena.Attributes`, `Cyrena.Contracts`, `Cyrena.Models`, `Cyrena.Options`, `Cyrena.Extensions`, `Cyrena.Components.Shared`

---

## Contracts (`Cyrena.Contracts`)

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

There is also an internal `ToolbarComponent` class (not public) that implements `IToolbarComponent`; the extension methods construct it via `new ToolbarComponent(typeof(TComponent), alignment)`.

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

- `DockRequest` is a nested record inside the interface.
- `Dock<T>` invokes a dock request with `Component = typeof(TKernelComponent)`. The active kernel must already be wired through the dock implementation.

### `IViewStartProvider`
Provides optional `ViewStart` entries for user-configurable starting views.

```csharp
public interface IViewStartProvider
{
    IEnumerable<ViewStart> Provide();
}
```

### `IFileAttacher` and abstract `FileAttacher`
Used to determine the component used for file attachments in a chat. Kernel-locked.

```csharp
public interface IFileAttacher : IComponent
{
    EventCallback<KernelContent[]> OnItemsAdded { get; set; }
}

public abstract class FileAttacher : KernelComponentBase, IFileAttacher
{
    [Parameter] public EventCallback<KernelContent[]> OnItemsAdded { get; set; }
}
```

The abstract `FileAttacher` provides a default `Parameter` for `OnItemsAdded`. Selection of the concrete type happens through `InterfaceOverrides.UseFileAttacher<T>()`.

### `IWindowLauncher`
Cross-platform browser window launcher. Implements `IDisposable`.

```csharp
public interface IWindowLauncher : IDisposable
{
    void Show(string url, int width, int height, string title = "Cyréna");
}
```

---

## Attributes (`Cyrena.Attributes`)

### `KernelInjectAttribute`
Indicates that the associated property should have a value injected from `KernelComponentBase.Kernel`.

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class KernelInjectAttribute : Attribute
{
    public object? Key { get; init; }
}
```

- `Key`: Optional keyed service key for `GetKeyedService` resolution.

### `ViewStartAttribute`
Marks a class as a possible starting view. Targets classes.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewStartAttribute : Attribute
{
    public ViewStartAttribute(string id, string title, string? description = null);
    public string Id { get; }
    public string Title { get; }
    public string? Description { get; }
}
```

---

## Models (`Cyrena.Models`)

### `KernelComponentBase`
Base class for Blazor components that need access to the current `Kernel` and its services via `[KernelInject]`.

```csharp
public abstract class KernelComponentBase : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Kernel Kernel
    {
        get { return _kernel; }
        set
        {
            _kernel = value;
            OnKernelSet();
        }
    }
}
```

- `Kernel` is `[Parameter]` and `[EditorRequired]`. The setter runs `OnKernelSet()` which reflects over public and non-public instance properties looking for `[KernelInject]`.
- For each marked property:
    - If `Key == null` → `Kernel.Services.GetService(prop.PropertyType)`
    - Otherwise → `Kernel.Services.GetKeyedService(prop.PropertyType, att.Key)`
- Throws `InvalidOperationException($"<name> has no setter.")` if a marked property has no setter.
- If the service is not registered, the property is set to `null` (no exception).
- The `BL0007` warning is suppressed intentionally because the kernel must be processed before `OnInitialized`.

### `ViewStart`
Information for a configurable starting view.

```csharp
public sealed class ViewStart
{
    public ViewStart(string id, Type componentType, string title, string? description);
    public string Id { get; }
    public Type ComponentType { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
}
```

Note: the actual properties are `Id`, `ComponentType`, `Title`, `Description` — there is **no** `Href` property. `Id` is a routing/identifier, `ComponentType` carries the Razor component to mount.

---

## Options (`Cyrena.Options`)

### `ComponentOptions` and `ComponentMetaData`
Configuration for UI components, particularly settings pages.

```csharp
public class ComponentOptions
{
    public ComponentOptions();
    internal List<ComponentMetaData> SettingsComponents { get; set; }

    public ComponentMetaData[] GetSettingsComponents();

    public MenuConfig MenuOptions { get; set; }   // initialised in ctor
    public FileSystemConfig FileSystemOptions { get; set; }   // initialised in ctor

    public static string OllamaDefaultEndpoint { get; set; } = "http://localhost:11434";
    public static bool IsServer { get; set; } = true;

    public class MenuConfig
    {
        public string ConverseUrl { get; set; } = "converse/{Id}";
        public bool AllowNewTab { get; set; } = true;
        public string GetConverseUrl(string id);   // returns ConverseUrl.Replace("{Id}", id)
    }

    public class FileSystemConfig
    {
        public bool ExploreFolder { get; set; } = true;
    }
}

public record ComponentMetaData(Type Component, string? Section, int Order);
```

- The constructor initialises `SettingsComponents`, `MenuOptions`, and `FileSystemOptions`. The default `MenuConfig.ConverseUrl` is `"converse/{Id}"` and `GetConverseUrl(id)` does a literal `Replace("{Id}", id)`.
- `OllamaDefaultEndpoint` and `IsServer` are static, not registered through DI.

### `ComponentOptionsExtensions` (same namespace)

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

- The `(string section)` overload defaults `order` to **10** (not 0 as previously documented).
- The `[Obsolete]` parameterless overload uses `Section = null, Order = 10`.
- All overloads check `!options.SettingsComponents.Any(x => x.Component == typeof(TComponent))` to prevent duplicates.

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

`GetFileLanguage` strips a leading `.` and lower-cases before lookup. The dictionary is private; no public accessor.

### `InterfaceOverrides`
Used to change default components in the chat interface. Kernel-locked.

```csharp
public sealed class InterfaceOverrides
{
    public void UseFileAttacher<TFileAttacher>() where TFileAttacher : class, IFileAttacher;
    public Type? FileAttacher { get; }
}
```

- `UseFileAttacher<T>()` records `typeof(T)` into a private backing field.
- `FileAttacher` exposes that type (or `null` if none was selected).
- Currently only the file attacher is overridable; the surface may grow.

---

## Extension Methods (`Cyrena.Extensions`)

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

- All three `AddSettingsComponent` overloads resolve `ComponentOptions` via `builder.GetFeatureOption<ComponentOptions>()` and delegate to `ComponentOptionsExtensions.AddSettingsComponent<T>`.
- The default `order` for the `(string section)` overload is **10** (delegated to `ComponentOptionsExtensions`).
- `AddShortcut<T>` registers `IShortcut` as **scoped**.

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

Both overloads register `builder.Services.AddSingleton<IToolbarComponent>(new ToolbarComponent(typeof(TComponent), alignment))`.

### `ComponentBaseExtensions`

```csharp
public static class ComponentBaseExtensions
{
    public static RenderFragment Render(this ComponentBase cmp, Type type);
    public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters);
}
```

- Sequence numbers start at 0 for `OpenComponent` and increment from 1 for each attribute.
- `cmp` is unused — present only as the extension-method anchor.
- The second overload uses `parameters.ElementAt(i)` (Linq), so the dictionary is enumerated — but only once at render time.

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

- Sets `FullWidth = true` automatically and forwards `maxWidth`.
- Returns `false` if `result == null` or `result.Canceled == true`; otherwise `true`.
- Returns `true` for any non-canceled result, including results where `Data` is null.

---

## Shared Blazor Components (`Cyrena.Components.Shared`)

### `CodeInput.razor`
Monaco code editor wrapper (BlazorMonaco) with syntax highlighting, dark theme, and two-way value binding.

**Parameters:**
- `Value` (`string?`) / `ValueChanged` (`EventCallback<string>`) — Two-way bound editor content
- `Language` (`string`, default `"plaintext"`) — Monaco language mode

**Behaviour:** `EditorConstructionOptions` returns `Theme = "vs-dark"`, `AutomaticLayout = true`. Also exposes a public `UpdateValue(string?)` method that calls `_code.SetValue(...)` inside a try/catch.

### `ConnectionSelector.razor`
MudBlazor `MudSelect<string>` of available AI connections from all registered `IConnectionProvider` services.

**Parameters:**
- `Value` / `ValueChanged` (`string?`) — Two-way bound selected connection ID
- `Label` (default `"AI Connection"`)
- `Required` (default `true`)

**Behaviour:** `OnInitialized` resolves `_providers = _services.GetServices<IConnectionProvider>()`. `OnAfterRenderAsync(firstRender)` calls `Populate()` which awaits `ListConnectionsAsync()` on each provider and clears-then-adds to `_models`, then calls `StateHasChanged()`. Options are displayed as `Name (Source)`. A leading `-- Select --` option maps to `null`.

### `PluginSelector.razor`
MudBlazor checkbox list for activating/deactivating `IAssistantPlugin` instances filtered by the current chat's assistant mode.

**Parameters:**
- `Chat` (`ChatConfiguration`, `[EditorRequired]`) — Chat whose `PluginIds` will be updated

**Behaviour:**
- `_services.GetServices<IAssistantPlugin>()` is filtered by `x.Modes.Length == 0 || x.Modes.Contains(Chat.AssistantModeId)`.
- If `Chat.PluginIds` is empty, all plugins start selected. Otherwise each plugin is selected if `Required` or its `Id` appears in `Chat.PluginIds`.
- `OnSelectionChanged` updates the local model and calls `PopulateChat()`, which writes `_models.Where(x => x.Selected || x.Required).Select(x => x.Id)` back into `Chat.PluginIds`.
- Required plugins (`IAssistantPlugin.Required == true`) are always checked and `Disabled` in the UI.

### `HistoryConfiguration.razor`
MudBlazor `MudSelect` for `ChatConfiguration.HistoryInclusion`.

**Parameters:**
- `Model` (`ChatConfiguration`) — Chat whose `HistoryInclusion` will be updated

**Options:** `All`, `Last 2 Iterations`, `Last 10 Iterations`, `Instruct (no history)`. The select is `Variant.Outlined`, `Dense`, `Required`, and shows a `HelperText` describing the selection.

---

## Usage for Extension Developers

Reference `Cyrena.Components.Core` to:
1. Implement `IShortcut` for quick actions
2. Implement `IToolbarComponent` indirectly by adding `KernelComponentBase`-derived components via `AddToolbarComponent`
3. Extend `KernelComponentBase` for kernel-aware UI components
4. Use `IDockingService` for docked panels
5. Use `[KernelInject]` for automatic service injection from kernel scope
6. Register settings components via `AddSettingsComponent`
7. Use `IViewStartProvider` plus `[ViewStartAttribute]` to expose starting views
8. Use `IFileAttacher` / `FileAttacher` to provide a custom file-attach component (select via `InterfaceOverrides.UseFileAttacher<T>`)
9. Use `DialogServiceExtensions.ShowDialogAsync<TComponent>()` for simple confirmation dialogs
10. Use `ComponentBaseExtensions.Render(...)` for dynamic component fragments
11. Use the shared `CodeInput`, `ConnectionSelector`, `PluginSelector`, `HistoryConfiguration` Blazor components directly
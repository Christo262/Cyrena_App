## Overview

`Cyrena.Components.Core` is the Blazor component library providing UI contracts, base classes, and extension methods for building UI components that integrate with Cyréna's kernel-scoped services. Extensions that add UI elements (toolbars, settings pages, shortcuts) must reference this package.

**Version:** 0.5.0
**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Contracts`, `Cyrena.Models`, `Cyrena.Options`, `Cyrena.Extensions`, `Cyrena.Attributes`, `Cyrena.Components.Shared`

---

## Contracts

### `IDisplayService` (Kernel-locked)
Service for showing dialogs, toast notifications, and navigation in the Blazor UI.

```csharp
public interface IDisplayService
{
    Task<DialogResult> ShowModal<TComponent>(ResultDialogOption option, Dialog? dialog = null) 
        where TComponent : IComponent, IResultDialog;
    
    Task<DialogResult> ShowModal(string title, string content, ResultDialogOption? option = null, Dialog? dialog = null);
    
    Task ShowToast(ToastOption option, ToastContainer? toastContainer = null);
    Task ShowErrorToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowWarnToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowSuccessToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowInfoToast(string? title = null, string? content = null, bool autoHide = true);
    
    void NavigateTo(string url);
}
```

**Dependencies:** Requires `BootstrapBlazor` components (`Dialog`, `ToastContainer`, `ToastOption`, `ResultDialogOption`, `DialogResult`).

**Implementation:** `DisplayService` in `Cyrena.Components.Runtime` implements this interface.

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
Indicates that the associated property should have a value injected from the `KernelComponentBase.Kernel` service provider. Overrides `ComponentBase.OnParametersSet`.

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

Components extending this class receive the active kernel instance as a parameter. When `Kernel` is set, all properties marked with `[KernelInject]` are automatically resolved from `Kernel.Services`.

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

- `SettingsComponents`: Collection of registered settings component metadata (internal).
- `GetSettingsComponents()`: Returns all registered settings components as an array.
- `ComponentMetaData`: Record describing a settings component with its type, section group, and display order.

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

- `AddSettingsComponent`: Registers a Blazor component as a settings tab under the specified section with optional order.
- `AddShortcut`: Registers a shortcut action in the UI as a scoped `IShortcut` service.

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

- `AddToolbarComponent`: Registers a component to render in the chat toolbar for the current kernel/chat.
- The obsolete overload on `IKernelBuilder` is deprecated; prefer `CyrenaKernelBuilder`.

### `ComponentBaseExtensions`

```csharp
public static class ComponentBaseExtensions
{
    public static RenderFragment Render(this ComponentBase cmp, Type type);
    public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters);
}
```

Helper methods for dynamically rendering Blazor components from code with optional parameter passing.

---

## Shared Blazor Components

### `CodeInput.razor`
Monaco code editor wrapper (BlazorMonaco) with syntax highlighting, dark theme, and two-way value binding.

**Parameters:**
- `Value` / `ValueChanged` — Two-way bound editor content
- `Language` — Monaco language mode (default: `"plaintext"`)

### `ConnectionSelector.razor`
Dropdown of available AI connections from all registered `IConnectionProvider` services.

**Parameters:**
- `Value` / `ValueChanged` — Two-way bound selected connection ID
- `Label` — Dropdown label (default: `"AI Connection"`)

**Behavior:** Populates connections on first render and on click. Displays `Name (Source)` per option.

### `PluginSelector.razor`
Checkbox list for activating/deactivating `IAssistantPlugin` instances filtered by the current chat's assistant mode.

**Parameters:**
- `Chat` (`ChatConfiguration`, required) — Chat whose `PluginIds` will be updated

**Behavior:**
- Filters plugins by mode compatibility
- Required plugins are always selected and disabled
- If `Chat.PluginIds` is empty, all plugins are selected by default
- Updates `Chat.PluginIds` on every selection change

---

## Usage for Extension Developers

Reference `Cyrena.Components.Core` to:
1. Implement `IShortcut` for quick actions
2. Implement `IToolbarComponent` for toolbar buttons
3. Extend `KernelComponentBase` for kernel-aware UI components
4. Use `IDisplayService` for dialogs and toasts
5. Use `[KernelInject]` for automatic service injection from kernel scope
6. Register settings components via `AddSettingsComponent`
7. Register toolbar components via `AddToolbarComponent`
8. Implement `IViewStartProvider` for custom starting views

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
    public string Icon => "fa-solid fa-star";
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

**Example - View Start Provider:**
```csharp
public class MyViewStartProvider : IViewStartProvider
{
    public IEnumerable<ViewStart> Provide()
    {
        yield return new ViewStart { Href = "/my-page", Title = "My Page" };
    }
}
```
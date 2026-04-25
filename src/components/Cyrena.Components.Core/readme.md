# Cyrena.Components SDK

**Core Dependency:** Cyrena.Core

Cyrena.Components is the UI framework library for the Cyrena AI assistant application, built on **BootstrapBlazor**. It provides reusable Blazor components, base classes for kernel-aware components, contract interfaces for UI integration points, and extension methods for component registration.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Cyrena.Application                          │
│                    (Consumer of SDK)                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Cyrena.Components                            │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Contracts (Integration Interfaces)                      │   │
│  │  • ICapability        - Component capability declaration │   │
│  │  • IShortcut          - UI shortcuts with actions        │   │
│  │  • IToolbarComponent   - Toolbar component registration   │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Models (Base Classes)                                   │   │
│  │  • CapabilityComponentBase - Base for capability comps  │   │
│  │  • KernelComponentBase      - Base for toolbar components│   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Options (Configuration)                                │   │
│  │  • ComponentOptions    - Router, navigation, settings    │   │
│  │  • CodeLanguages        - File extension → language map   │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Extensions (Registration)                               │   │
│  │  • CyrenaBuilderExtensions  - App-level registration    │   │
│  │  • KernelBuilderExtensions  - Kernel-level registration │   │
│  │  • ComponentBaseExtensions   - Render helpers            │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Services (Internal)                                     │   │
│  │  • ComponentAssistantsPlugin - Adds BootstrapBlazor      │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Cyrena.Core                                  │
│         (Semantic Kernel infrastructure)                      │
└─────────────────────────────────────────────────────────────────┘
```

---

## Contracts

### ICapability
Declares a capability component type. Used to register components that extend AI assistant capabilities.

```csharp
namespace Cyrena.Contracts
{
    public interface ICapability
    {
        Type Component { get; }
    }

    internal class Capability : ICapability
    {
        public Capability(Type component);
        public Type Component { get; }
    }
}
```

### IShortcut
Defines a keyboard shortcut with visual metadata and click handler.

```csharp
namespace Cyrena.Contracts
{
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
}
```

### IToolbarComponent
Registers a component for display in the chat toolbar.

```csharp
namespace Cyrena.Contracts
{
    public interface IToolbarComponent
    {
        Type Component { get; }
        ToolbarAlignment Alignment { get; }
    }

    public enum ToolbarAlignment
    {
        Start,
        End
    }

    internal class ToolbarComponent : IToolbarComponent
    {
        public ToolbarComponent(Type component, ToolbarAlignment alignment);
        public Type Component { get; }
        public ToolbarAlignment Alignment { get; }
    }
}
```

---

## Models

### CapabilityComponentBase
Abstract base class for capability components. These components are invoked by the AI to perform specialized tasks.

```csharp
namespace Cyrena.Models
{
    public abstract class CapabilityComponentBase : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public Kernel Kernel { get; set; }

        [Parameter]
        public EventCallback<AdditionalMessageContent[]> OnItemsAdded { get; set; }
    }
}
```

**Usage:** Create subclasses to implement AI-triggered capabilities (e.g., file operations, code execution, search).

### KernelComponentBase
Abstract base class for kernel-aware components used in the UI toolbar.

```csharp
namespace Cyrena.Models
{
    public abstract class KernelComponentBase : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public Kernel Kernel { get; set; }
    }
}
```

**Usage:** Create subclasses for toolbar buttons, menus, or other UI elements that need access to the Semantic Kernel.

---

## Options

### ComponentOptions
Configuration for Blazor routing, navigation components, and settings components.

```csharp
namespace Cyrena.Options
{
    public class ComponentOptions
    {
        public ComponentOptions();

        internal List<Assembly> RouterAssemblies { get; set; }
        internal List<Type> NavigationComponents { get; set; }
        internal List<Type> SettingsComponents { get; set; }

        public Assembly[] GetRouterAssemblies();
        public Type[] GetNavigationComponents();
        public Type[] GetSettingsComponents();
    }

    public static class ComponentOptionsExtensions
    {
        public static void AddRouterAssembly(this ComponentOptions options, Assembly assembly);
        public static void AddRouterAssembly<T>(this ComponentOptions options);
        public static void AddNavigationComponent<TComponent>(this ComponentOptions options) where TComponent : ComponentBase;
        public static void AddSettingsComponent<TComponent>(this ComponentOptions options) where TComponent : ComponentBase;
    }
}
```

### CodeLanguages
Maps file extensions to Pygments-compatible language identifiers for syntax highlighting in code editors.

```csharp
namespace Cyrena.Options
{
    public class CodeLanguages
    {
        public CodeLanguages();

        public string GetFileLanguage(string extension);

        // Supported mappings:
        // c, h → c
        // cpp, hpp, ino → cpp
        // cs → csharp
        // razor → html
        // css → css
        // js → javascript
        // md → markdown
        // csproj, xml → xml
        // json → json
        // (unmatched) → plaintext
    }
}
```

---

## Extension Methods

### CyrenaBuilderExtensions
Application-level registration for UI components.

```csharp
namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        // Initialize the UI framework (call once during startup)
        public static CyrenaBuilder AddComponents(this CyrenaBuilder builder);

        // Register a navigation component (appears in nav bar)
        public static CyrenaBuilder AddNavigationComponent<TComponent>(this CyrenaBuilder builder) 
            where TComponent : ComponentBase;

        // Register a settings component (appears in settings panel)
        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder) 
            where TComponent : ComponentBase;

        // Add assemblies for Blazor router discovery
        public static CyrenaBuilder AddRouterAssembly<T>(this CyrenaBuilder builder);

        // Register a keyboard shortcut
        public static CyrenaBuilder AddShortcut<TShortcut>(this CyrenaBuilder builder) 
            where TShortcut : class, IShortcut;
    }
}
```

### KernelBuilderExtensions
Kernel-level registration for capability and toolbar components.

```csharp
namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        // Register a capability component
        public static void AddCapability<TComponent>(this IKernelBuilder builder) 
            where TComponent : CapabilityComponentBase;

        // Register a toolbar component with alignment
        public static void AddToolbarComponent<TComponent>(this IKernelBuilder builder, ToolbarAlignment alignment) 
            where TComponent : KernelComponentBase;
    }
}
```

### ComponentBaseExtensions
Render fragment helpers for dynamic component rendering.

```csharp
namespace Cyrena.Extensions
{
    public static class ComponentBaseExtensions
    {
        // Render a component with no parameters
        public static RenderFragment Render(this ComponentBase cmp, Type type);

        // Render a component with parameters
        public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters);
    }
}
```

---

## Services

### ComponentAssistantsPlugin (Internal)
Internal `IAssistantPlugin` implementation that bootstraps BootstrapBlazor integration.

```csharp
namespace Cyrena.Services
{
    internal class ComponentAssistantsPlugin : IAssistantPlugin
    {
        public ComponentAssistantsPlugin(DialogService dialog, ToastService toasts);

        public string[] Modes => [];

        public int Priority => 10;

        public Task LoadAsync(CyrenaKernelBuilder builder);
    }
}
```

**What it does:**
1. Registers `DialogService` and `ToastService` in the kernel service collection
2. Adds the `ExportChat` component as a toolbar component with `ToolbarAlignment.End`
3. Automatically called when `AddComponents()` is invoked on the builder

---

## Blazor Integration

### _Imports.razor
Standard using directives for all Blazor components:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using BootstrapBlazor.Components
@using Cyrena.Models
@using Cyrena.Contracts
```

---

## Usage Examples

### Registering Navigation Components

```csharp
// In Program.cs or startup
builder.AddComponents();
builder.AddNavigationComponent<MyNavComponent>();
builder.AddSettingsComponent<MySettingsComponent>();
```

### Creating a Capability Component

```csharp
public class FileSearchCapability : CapabilityComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        // Access Kernel for AI integration
        // Use OnItemsAdded to emit content to chat
    }
}

// Register in startup
kernelBuilder.AddCapability<FileSearchCapability>();
```

### Creating a Toolbar Component

```csharp
public class ExportButton : KernelComponentBase
{
    [Inject]
    private DialogService? Dialog { get; set; }

    protected async Task OnClick()
    {
        // Use Kernel context if needed
        // Show dialogs using DialogService
    }
}

// Register in plugin
builder.AddToolbarComponent<ExportButton>(ToolbarAlignment.End);
```

### File Language Detection

```csharp
var languages = new CodeLanguages();
string lang = languages.GetFileLanguage(".cs");      // "csharp"
string lang = languages.GetFileLanguage("hpp");      // "cpp"
string lang = languages.GetFileLanguage(".unknown"); // "plaintext"
```

---

## Package Dependencies

- **BootstrapBlazor** - UI component library
- **BootstrapBlazor.Html2Pdf** - PDF generation
- **Microsoft.AspNetCore.Components.Web** - Blazor web assembly support
- **BlazorMonaco** - Code editor component
- **Markdig** - Markdown parsing
- **PdfPig** - PDF text extraction
- **Cyrena.Core** - Core Semantic Kernel infrastructure

---

## NuGet Package

```
Package: Cyrena.Components
Version: 0.3.0
Authors: Vaya Nova
Description: Key components and shared components for Cyrena AI assistant
Target: browser (WebAssembly)
```

The package is automatically copied to `../../../sdk` after build via the `PostPack` target.
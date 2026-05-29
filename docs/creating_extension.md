## Overview

This document is a complete, grounded guide for building a Cyréna extension. It uses **Cyrena.Tavily** (the Tavily web search plugin) as the concrete, real-world example. Every code snippet and pattern shown here is derived from actual source code.

An extension in Cyréna can serve multiple purposes:
- Add AI capabilities (plugins, tools, prompts)
- Add UI components (settings panels, pages, shared components)
- Add persistence backends
- Add LLM connection providers

Extensions can be:
- **Compile-time**: Referenced as project references, loaded at startup
- **Runtime**: Distributed as ZIP packages, loaded dynamically via Extensa

---

## Project Structure

A typical extension project follows the standard Cyréna class library structure:

```
MyExtension/
├── MyExtension.csproj              # SDK: Microsoft.NET.Sdk.Razor
├── extension.json                  # Extension manifest (required for runtime)
├── TavilyExtension.cs              # Extension entry point (inherits Extension)
├── Extensions/
│   └── CyrenaBuilderExtensions.cs  # Builder extension methods
├── Services/
│   └── TavilyExtension.cs          # IAssistantPlugin implementation
├── Models/
│   ├── SearchRequest.cs            # API request models
│   ├── SearchResponse.cs           # API response models
│   └── ...
├── Options/
│   └── TavilyOptions.cs            # Settings/options class
├── Components/
│   ├── _Imports.razor              # Razor imports
│   └── Shared/
│       └── TavilySettings.razor    # Settings UI component
└── Resources/
    └── prompt.md                   # Embedded system prompt
```

---

## Step 1: Project File (MyExtension.csproj)

Extensions that add UI components must use the Razor SDK:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Required for Blazor components -->
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" />
  </ItemGroup>

  <ItemGroup>
    <!-- Reference the core extension system -->
    <ProjectReference Include="..\..\extensa\Cyrena.Extensa.Core\Cyrena.Extensa.Core.csproj" />
    <!-- Reference if adding UI components -->
    <ProjectReference Include="..\..\components\Cyrena.Components.Core\Cyrena.Components.Core.csproj" />
  </ItemGroup>
</Project>
```

**Key points:**
- Use `Microsoft.NET.Sdk.Razor` if the extension contains `.razor` components
- Reference `Cyrena.Extensa.Core` for the `Extension` base class
- Reference `Cyrena.Components.Core` for UI contracts like `ISettingsComponent`

---

## Step 2: Extension Manifest (extension.json)

The `extension.json` file is **required** for runtime-loaded extensions. It describes the extension to the Extensa loader:

```json
{
  "id": "cyrena.tavily",
  "name": "Tavily Web Search",
  "version": "0.4.0",
  "entryAssemblyFile": "Cyrena.Tavily.dll",
  "description": "Adds web search functionality for assistants using Tavily.",
  "icon": null,
  "contentRootDirectory": null,
  "dependencies": [
    {
      "id": "cyrena",
      "minVersion": "0.4.0"
    }
  ],
  "requireFrameworkBuilder": true
}
```

**Fields:**
| Field | Required | Description |
|-------|----------|-------------|
| `id` | Yes | Unique identifier, reverse-domain recommended |
| `name` | Yes | Human-readable name |
| `version` | Yes | SemVer version string |
| `entryAssemblyFile` | Yes | Main DLL filename |
| `description` | No | Short description |
| `icon` | No | Icon path or null |
| `contentRootDirectory` | No | Static content root (for wwwroot assets) |
| `dependencies` | No | Array of `{ "id": "...", "minVersion": "..." }` |
| `requireFrameworkBuilder` | No | If true, requires framework builder context |

---

## Step 3: Extension Entry Point

The entry point class inherits from `Extension` (from `Cyrena.Extensa.Core`) and overrides `BuildExtension`:

```csharp
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Tavily
{
    public class TavilyExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddTavily();
        }
    }
}
```

**How it works:**
- The Extensa loader discovers this type via reflection
- Calls `BuildExtension(CyrenaBuilder)` during application startup
- The builder is the same `CyrenaBuilder` used to configure the main app
- All registrations (plugins, components, services) are additive

---

## Step 4: Builder Extension Methods

Create a static extension class on `CyrenaBuilder` to encapsulate all registrations:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Tavily.Components.Shared;
using Cyrena.Tavily.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddTavily(this CyrenaBuilder builder)
        {
            // Register the plugin (adds AI capabilities)
            builder.AddAssistantPlugin<TavilyExtension>();
            
            // Register a settings UI component
            builder.AddSettingsComponent<TavilySettings>();

            return builder;
        }
    }
}
```

**Available builder methods for extensions:**
- `builder.AddAssistantPlugin<T>()` - Registers an `IAssistantPlugin`
- `builder.AddAssistantMode<T>()` - Registers an `IAssistantMode`
- `builder.AddSettingsComponent<T>()` - Registers a settings UI component
- `builder.AddConnectionProvider<T>()` - Registers an LLM connection provider
- `builder.AddStore<T>()` - Registers a persistence store
- `builder.Services.Add...` - Direct DI service registration

---

## Step 5: IAssistantPlugin Implementation

The plugin is where AI capabilities are added. It implements `IAssistantPlugin`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Tavily.Options;
using Cyrena.Tavily.Plugins;
using Cyrena.Models;
using Cyrena.Extensions;

namespace Cyrena.Tavily.Services
{
    internal class TavilyExtension : IAssistantPlugin
    {
        private readonly ISettingsService _settings;
        
        public TavilyExtension(ISettingsService settings)
        {
            _settings = settings;
        }

        public int Priority => 10;
        public string[] Modes => [];  // Empty = compatible with ALL modes
        public string Id => "cyrena.tavily";
        public bool Required => false;
        public string Title => "Tavily Web Search";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            // Read settings from encrypted storage
            var options = _settings.Read<TavilyOptions>(TavilyOptions.Key);
            
            // Guard: skip loading if not configured
            if (options == null || string.IsNullOrEmpty(options.ApiKey) || !options.Enable)
                return Task.CompletedTask;
            
            // Register options in the kernel's DI container
            builder.Services.AddSingleton(options);
            
            // Add Semantic Kernel plugin (exposes functions to the AI)
            builder.Plugins.AddFromType<Internet>();
            
            // Add a system prompt to guide the AI's behavior
            var prompt = Resources.Read(typeof(TavilyExtension).Assembly, "Cyrena.Tavily.Resources.prompt.md");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(10, prompt);
            
            return Task.CompletedTask;
        }
    }
}
```

**IAssistantPlugin contract:**
| Member | Type | Description |
|--------|------|-------------|
| `Priority` | `int` | Load order (lower = earlier). Default is 0. |
| `Modes` | `string[]` | Compatible mode IDs. Empty array = all modes. |
| `Id` | `string` | Unique plugin identifier |
| `Required` | `bool` | If true, cannot be disabled by user |
| `Title` | `string` | Human-readable name |
| `LoadAsync(CyrenaKernelBuilder)` | `Task` | Called per-chat when kernel is initialized |

**What you can do in `LoadAsync`:**
- Read settings via `ISettingsService`
- Register services in `builder.Services`
- Add Semantic Kernel plugins via `builder.Plugins.AddFromType<T>()`
- Add system prompts via `builder.GetFeatureOption<IPromptManager>().AddPrompt(priority, prompt)`
- Access the kernel builder's feature options

---

## Step 6: Options / Settings Class

Options classes define user-configurable settings. They use the Options pattern with a `Key` constant:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Tavily.Options
{
    public class TavilyOptions
    {
        public const string Key = "tavily";
        
        [Required]
        public string? ApiKey { get; set; }
        
        public bool Enable { get; set; }
    }
}
```

**Reading settings:**
```csharp
var options = settingsService.Read<TavilyOptions>(TavilyOptions.Key);
```

**Writing settings:**
```csharp
settingsService.Write(TavilyOptions.Key, new TavilyOptions { ApiKey = "...", Enable = true });
```

Settings are automatically encrypted at rest using platform-specific encryption (DPAPI on Windows).

---

## Step 7: Models

Models for API communication should inherit from `JsonStringObject` (from `Cyrena.Core`) for easy JSON serialization:

```csharp
using Cyrena.Models;

namespace Cyrena.Tavily.Models
{
    public class SearchRequest : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("query")]
        public string? Query { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("topic")]
        public string? Topic { get; set; } // general, news, finance
        
        [System.Text.Json.Serialization.JsonPropertyName("search_depth")]
        public string? SearchDepth { get; set; } // basic, advanced
        
        [System.Text.Json.Serialization.JsonPropertyName("max_results")]
        public int MaxResults { get; set; } = 5;
        
        [System.Text.Json.Serialization.JsonPropertyName("include_images")]
        public bool IncludeImages { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("include_image_descriptions")]
        public bool IncludeImageDescriptions { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("include_raw_content")]
        public string? IncludeRawContent { get; set; } // none, text, markdown
    }
}
```

**`JsonStringObject` base class:**
- Provides `ToString()` override that returns indented JSON
- Used for models that need to be serialized to JSON strings
- Located in `Cyrena.Models` namespace

---

## Step 8: UI Components (Optional)

Extensions can add Blazor components. The `_Imports.razor` file scopes imports:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using BootstrapBlazor.Components
@using Cyrena.Contracts
@using Cyrena.Tavily.Models
@using Cyrena.Tavily.Options
```

**Settings component example** (registered via `builder.AddSettingsComponent<TavilySettings>()`):
- Inherits from `ComponentBase` or `KernelComponentBase`
- Uses `ISettingsService` injected via `[Inject]`
- Renders a form for configuring the extension's options
- The component is discovered and displayed in the app's settings panel

---

## Step 9: Embedded Resources

Extensions can embed resources (like system prompts) that are read at runtime:

```xml
<!-- In .csproj -->
<ItemGroup>
  <EmbeddedResource Include="Resources\prompt.md" />
</ItemGroup>
```

**Reading embedded resources:**
```csharp
var prompt = Resources.Read(typeof(TavilyExtension).Assembly, "Cyrena.Tavily.Resources.prompt.md");
builder.GetFeatureOption<IPromptManager>().AddPrompt(10, prompt);
```

The `Resources.Read` helper (from `Cyrena.Core`) reads embedded resources by fully-qualified name.

---

## Complete Extension Lifecycle

```
1. App starts
   └── CyrenaBuilder.Build() called
       └── All compile-time extensions: BuildExtension(builder) called
           └── builder.AddTavily() registers plugin + settings component

2. User creates a new chat
   └── IKernelController creates kernel
       └── CyrenaKernelBuilder instantiated
           └── IAssistantPlugin.LoadAsync(builder) called for each active plugin
               └── TavilyExtension reads settings, adds SK plugin, adds prompt

3. User sends a message
   └── Semantic Kernel processes with Tavily plugin functions available
       └── AI can call web search functions as needed
```

---

## Key Patterns Summary

| Pattern | Implementation |
|---------|---------------|
| **Entry Point** | Class inheriting `Extension`, overriding `BuildExtension` |
| **Builder Extension** | Static `CyrenaBuilder AddXxx(this CyrenaBuilder builder)` |
| **AI Plugin** | Class implementing `IAssistantPlugin` |
| **Settings** | Options class with `const string Key`, read via `ISettingsService` |
| **Models** | Inherit from `JsonStringObject` for JSON serialization |
| **UI Components** | Blazor components registered via `AddSettingsComponent<T>()` |
| **Embedded Resources** | `<EmbeddedResource>` in csproj, read via `Resources.Read()` |
| **Manifest** | `extension.json` with id, version, dependencies |

---

## Required Package References

| Package | Purpose |
|---------|---------|
| `Cyrena.Extensa.Core` | Extension base class, `Extension` |
| `Cyrena.Core` | Builders, contracts, models, `IAssistantPlugin` |
| `Cyrena.Components.Core` | UI contracts, `ISettingsComponent`, `KernelComponentBase` |
| `Microsoft.AspNetCore.Components.Web` | Blazor component support |
| `Microsoft.SemanticKernel` | Semantic Kernel plugins (if adding AI tools) |

---

## File Checklist for New Extensions

- [ ] `extension.json` - Manifest with id, version, entryAssemblyFile
- [ ] `MyExtension.cs` - Entry point class inheriting `Extension`
- [ ] `Extensions/CyrenaBuilderExtensions.cs` - Builder extension method
- [ ] `Services/MyPlugin.cs` - `IAssistantPlugin` implementation
- [ ] `Options/MyOptions.cs` - Settings/options class with Key constant
- [ ] `Models/` - Request/response models inheriting `JsonStringObject`
- [ ] `Components/Shared/MySettings.razor` - Settings UI (optional)
- [ ] `Resources/` - Embedded prompts, templates (optional)
- [ ] `.csproj` - Razor SDK, project references, embedded resources
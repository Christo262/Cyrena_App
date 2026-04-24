# Cyrena.Extensa.Core SDK

**Version:** 0.3.0  
**Target Framework:** .NET 10.0  
**Root Namespace:** `Cyrena.Extensa`  
**Core Dependency:** `Cyrena.Core`

Cyrena.Extensa.Core is the extension/plugin SDK for the Cyrena AI assistant application. It provides the **contract interface** (`IExtension`) and **models** that actual extensions must implement to be discovered and loaded into the Cyrena application runtime. **This library does not provide application functionality itself** — it defines the contract that third-party extensions must follow.

---

## Relationship Between Libraries

```
┌─────────────────────────────────────────────────────────────────┐
│                    Cyrena Application                            │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    Cyrena.Core                           │   │
│  │  (Provides Semantic Kernel, chat management, LLM        │   │
│  │   connections, and core application services)           │   │
│  └─────────────────────────────────────────────────────────┘   │
│                           ▲                                     │
│                           │ Uses CyrenaBuilder                 │
│                           │ for registration                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               Cyrena.Extensa.Core                        │   │
│  │  (This library)                                          │   │
│  │                                                          │   │
│  │  Provides:                                               │   │
│  │  • IExtension (contract for plugins)                     │   │
│  │  • Extension (abstract base class)                      │   │
│  │  • ExtensionInfo (plugin metadata)                      │   │
│  │  • Dependency (versioned dependency model)              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                           ▲                                     │
│                           │ Implements IExtension              │
│                           │ to register services               │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               Actual Extensions/Plugins                 │   │
│  │  (Your code that uses Cyrena.Core services via          │   │
│  │   IExtension.BuildExtension(CyrenaBuilder))             │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Contracts

### IExtension

The **primary contract** that all extensions must implement to be loaded into the Cyrena application.

```csharp
namespace Cyrena.Extensa.Contracts
{
    /// <summary>
    /// The interface all extensions must implement
    /// </summary>
    public interface IExtension
    {        
        /// <summary>
        /// Called by the Extensa.Loader to add the extension to the application dependencies
        /// </summary>
        /// <param name="builder"><see cref="CyrenaBuilder"/></param>
        void BuildExtension(CyrenaBuilder builder);
    }
}
```

**Usage Pattern:**
Extensions implement `IExtension` and use the `BuildExtension` method to register their services into the dependency injection container via the `CyrenaBuilder`.

**Example Implementation:**
```csharp
using Cyrena.Extensa.Contracts;
using Cyrena.Options;

public class MyFeatureExtension : IExtension
{
    public void BuildExtension(CyrenaBuilder builder)
    {
        // Register services, modes, plugins, or startup tasks
        builder.AddAssistantPlugin<MyAssistantPlugin>("my-mode");
        builder.AddStartupTask<MyFeatureInitializer>();
        builder.AddFeatureAssembly(typeof(MyFeatureExtension).Assembly);
    }
}
```

---

## Models

### ExtensionInfo

Metadata model containing information about a specific extension. Used for extension discovery, versioning, and dependency resolution.

```csharp
namespace Cyrena.Extensa.Models
{
    public class ExtensionInfo
    {
        /// <summary>
        /// Unique identifier for the extension
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Human-readable display name
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Optional description of what the extension provides
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional icon identifier or path
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Semantic version of the extension (defaults to "1.0.0")
        /// </summary>
        public Version Version { get; set; } = Version.Parse("1.0.0");

        /// <summary>
        /// Path to the extension's entry assembly file
        /// </summary>
        public string? EntryAssemblyFile { get; set; }

        /// <summary>
        /// Content root directory for the extension's static files/resources
        /// </summary>
        public string? ContentRootDirectory { get; set; }

        /// <summary>
        /// Array of dependencies required by this extension
        /// </summary>
        public Dependency[] Dependencies { get; set; } = [];

        /// <summary>
        /// Whether this extension requires the framework builder (defaults to true)
        /// </summary>
        public bool RequireFrameworkBuilder { get; set; } = true;
    }
}
```

**Default Values:**
- `Version`: 1.0.0
- `Dependencies`: Empty array
- `RequireFrameworkBuilder`: true

---

### Dependency

Model representing a versioned dependency on another extension.

```csharp
namespace Cyrena.Extensa.Models
{
    public class Dependency
    {
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Dependency() { }

        /// <summary>
        /// Constructor with id and minimum version
        /// </summary>
        /// <param name="id">The extension ID this depends on</param>
        /// <param name="minVersion">Minimum required version</param>
        public Dependency(string id, Version minVersion)
        {
            Id = id;
            MinVersion = minVersion;
        }

        /// <summary>
        /// The extension ID of the dependency
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Minimum version required for the dependency
        /// </summary>
        public Version MinVersion { get; set; } = default!;
    }
}
```

**Usage Example:**
```csharp
public class MyExtension : IExtension
{
    public void BuildExtension(CyrenaBuilder builder)
    {
        // ExtensionInfo with dependency
    }
}

// In a manifest or configuration:
var extensionInfo = new ExtensionInfo
{
    Id = "my-feature",
    Name = "My Feature",
    Version = Version.Parse("1.2.0"),
    Dependencies = new[]
    {
        new Dependency("cyrena-auth", Version.Parse("1.0.0")),
        new Dependency("cyrena-storage", Version.Parse("2.0.0"))
    }
};
```

---

### Extension (Abstract Base Class)

Abstract base class implementing `IExtension` for convenience. Provides an empty default implementation of `BuildExtension` so derived classes only override what they need.

```csharp
namespace Cyrena.Extensa.Models
{
    public abstract class Extension : IExtension
    {
        /// <summary>
        /// Default implementation does nothing. Override to register services.
        /// </summary>
        /// <param name="builder"><see cref="CyrenaBuilder"/></param>
        public virtual void BuildExtension(CyrenaBuilder builder)
        {
        }
    }
}
```

**Usage Pattern:**
```csharp
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Options;

public class MyExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        builder.AddAssistantPlugin<MyPlugin>("my-mode");
    }
}
```

---

## Options (Referenced Types)

### CyrenaBuilder

Used within `IExtension.BuildExtension()` to register extension services. This type is defined in `Cyrena.Core` and represents the main application builder.

**Key members used by extensions:**

```csharp
public class CyrenaBuilder
{
    /// <summary>
    /// Feature assemblies for plugin discovery
    /// </summary>
    public IList<Assembly> FeatureAssemblies { get; }

    /// <summary>
    /// Feature options per feature ID
    /// </summary>
    public IDictionary<string, FeatureOptions> FeatureOptions { get; }

    /// <summary>
    /// Lifecycle hooks executed after IServiceProvider is built
    /// </summary>
    public IList<Action<IServiceProvider>> BuildActions { get; }

    /// <summary>
    /// Lifecycle hooks executed during application run
    /// </summary>
    public IList<Func<IServiceProvider, CancellationToken, Task>> RunActions { get; }
}
```

---

## Extension Methods (Cyrena.Core)

Extensions typically use these extension methods (defined in `Cyrena.Core`) to register their functionality:

```csharp
public static class CyrenaBuilderExtensions
{
    /// <summary>
    /// Add feature options for a specific feature ID
    /// </summary>
    public static CyrenaBuilder AddFeatureOptions(this CyrenaBuilder builder, string featureId, FeatureOptions options);

    /// <summary>
    /// Add an assembly for feature/plugin discovery
    /// </summary>
    public static CyrenaBuilder AddFeatureAssembly(this CyrenaBuilder builder, Assembly assembly);

    /// <summary>
    /// Add a startup task that executes after IServiceProvider is built
    /// </summary>
    public static CyrenaBuilder AddStartupTask<T>(this CyrenaBuilder builder) where T : class, IStartupTask;

    /// <summary>
    /// Add an assistant mode directly
    /// </summary>
    public static CyrenaBuilder AddAssistantMode<T>(this CyrenaBuilder builder, string modeId) where T : class, IAssistantMode;

    /// <summary>
    /// Add an assistant plugin (registers IAssistantPlugin as a DI service)
    /// </summary>
    public static CyrenaBuilder AddAssistantPlugin<T>(this CyrenaBuilder builder, string modeId) where T : class, IAssistantPlugin;
}
```

---

## Extension Loading Architecture

### Loading Flow

```
1. Application starts
   │
2. Cyrena.Extensa.Loader (external component) discovers extension assemblies
   │
3. For each discovered extension:
   │
   a) Load ExtensionInfo from assembly metadata
   │
   b) Check dependency versions are satisfied
   │
 c) Create instance of IExtension implementation
   │
 d) Call BuildExtension(cyrenaBuilder) to register services
   │
 e) Store ExtensionInfo for runtime management
```

### Extension Discovery

Extensions are discovered through:
- Assembly scanning by the `Extensa.Loader` component
- Metadata stored in `ExtensionInfo` attached to each extension assembly

### Dependency Resolution

Before loading an extension:
1. All dependencies are checked via `Dependency.MinVersion`
2. Missing dependencies prevent extension loading
3. Version mismatches prevent extension loading

---

## Complete Extension Example

```csharp
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Options;

namespace MyCompany.CyrenaFeatures;

public class MyFeatureExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        // Register the assembly for further plugin discovery
        builder.AddFeatureAssembly(typeof(MyFeatureExtension).Assembly);
        
        // Register an assistant plugin for kernel configuration
        builder.AddAssistantPlugin<MyFeaturePlugin>("my-feature-mode");
        
        // Register a startup task for initialization
        builder.AddStartupTask<MyFeatureInitializer>();
        
        // Add feature-specific options
        builder.AddFeatureOptions("my-feature", new MyFeatureOptions());
    }
}

// Assistant Plugin implementation
public class MyFeaturePlugin : IAssistantPlugin
{
    public IReadOnlyList<string> Modes => ["my-feature-mode"];
    public int Priority => 100;
    
    public Task LoadAsync(CyrenaKernelBuilder builder, CancellationToken ct)
    {
        // Add Semantic Kernel services
        builder.KernelBuilder.Plugins.AddFromType<MyKernelPlugin>();
        return Task.CompletedTask;
    }
    
    public Task ConfigureAsync(CyrenaKernelBuilder builder, CancellationToken ct)
        => LoadAsync(builder, ct);
}

// Startup task for feature initialization
public class MyFeatureInitializer : IStartupTask
{
    public int Order => 100;
    
    public async Task ExecuteAsync(IServiceProvider sp, CancellationToken ct)
    {
        // Perform initialization work
        var settings = sp.GetRequiredService<ISettingsService>();
        await settings.SaveAsync("my-feature", new { Enabled = true });
    }
}
```

---

## Package Information

**NuGet Package:** `Cyrena.Extensa.Core`  
**Version:** 0.3.0  
**Authors:** Vaya Nova  
**Description:** Core models, interfaces & services to develop extensions for Cyrena

### Package Dependencies

- `Cyrena.Core` (0.3.0) - Core application framework
- `Microsoft.SemanticKernel` - Semantic Kernel integration
- `Microsoft.Extensions.Configuration` - Configuration support
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI abstractions
- `Microsoft.Extensions.Options` - Options pattern

---

## Summary

| Type | Purpose |
|------|---------|
| `IExtension` | **Contract interface** - All extensions must implement this to be loaded |
| `Extension` | **Abstract base class** - Convenience base with empty `BuildExtension` |
| `ExtensionInfo` | **Metadata model** - Id, Name, Version, Dependencies, etc. |
| `Dependency` | **Dependency model** - Id and minimum version for another extension |

**Key Principle:** Cyrena.Extensa.Core provides only the **contract** and **models** for extension development. Actual extension functionality (services, modes, plugins) is defined in `Cyrena.Core` and consumed via the `CyrenaBuilder` passed to `BuildExtension`.

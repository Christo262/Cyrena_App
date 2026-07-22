## Overview

`Cyrena.Extensa.Core` is the core extension system for Cyréna's dynamic plugin framework. It defines the contracts and models that all extensions must implement to be discovered and loaded at runtime by the Extensa loader. Extensions are compiled as separate assemblies/DLLs and loaded dynamically with dependency resolution.

**Version:** 0.6.0
**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Extensa.Contracts`, `Cyrena.Extensa.Models`

---

## Contracts

### `IExtension`
The interface all extensions must implement. This is the single entry point for an extension to integrate with Cyréna.

```csharp
namespace Cyrena.Extensa.Contracts
{
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

**Key behaviors:**
- `BuildExtension` is called during application startup by the Extensa loader.
- The `CyrenaBuilder` parameter provides access to `Services`, `FeatureOptions`, `FeatureAssemblies`, `BuildActions`, and `RunActions`.
- Use this method to register services, add assistant modes, add plugins, configure persistence stores, register UI components, etc.
- The extension assembly must contain at least one type implementing `IExtension`.

---

## Models

### `Extension` (Abstract Base)
Convenience abstract base class implementing `IExtension` with an empty `BuildExtension` method.

```csharp
namespace Cyrena.Extensa.Models
{
    public abstract class Extension : IExtension
    {
        public virtual void BuildExtension(CyrenaBuilder builder)
        {
        }
    }
}
```

Extend this class if your extension needs no build-time configuration, or override `BuildExtension` to add custom setup.

### `ExtensionInfo`
Metadata describing an extension package. Used by the Extensa loader for discovery, dependency resolution, and UI display.

```csharp
namespace Cyrena.Extensa.Models
{
    public class ExtensionInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("version")]
        public Version Version { get; set; } = Version.Parse("1.0.0");
        [JsonPropertyName("entryAssemblyFile")]
        public string? EntryAssemblyFile { get; set; }
        [JsonPropertyName("dependencies")]
        public Dependency[] Dependencies { get; set; } = [];
    }
}
```

**Properties:**
- `Id` (`string`) — Unique identifier for the extension (e.g., `com.example.myextension`). JSON: `id`.
- `Name` (`string`) — Human-readable name. JSON: `name`.
- `Description` (`string?`) — Optional description. JSON: `description`.
- `Version` (`Version`) — Extension version. Defaults to `1.0.0`. JSON: `version`.
- `EntryAssemblyFile` (`string?`) — The main DLL file name containing the `IExtension` implementation. JSON: `entryAssemblyFile`.
- `Dependencies` (`Dependency[]`) — Array of `Dependency` objects specifying required extensions. Defaults to empty array. JSON: `dependencies`.

### `Dependency`
Represents a dependency on another extension with minimum version requirements.

```csharp
namespace Cyrena.Extensa.Models
{
    public class Dependency
    {
        public Dependency() { }
        public Dependency(string id, Version minVersion)
        {
            Id = id;
            MinVersion = minVersion;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("minVersion")]
        public Version MinVersion { get; set; } = default!;
    }
}
```

**Constructors:**
- `Dependency()` — Parameterless ctor for deserialization.
- `Dependency(string id, Version minVersion)` — Ctor taking id and minimum version.

**Properties:**
- `Id` (`string`) — The `Id` of the required extension. JSON: `id`.
- `MinVersion` (`Version`) — The minimum required version. JSON: `minVersion`.

---

## Extension Loading Process

1. **Discovery**: The Extensa loader scans extension directories for `ExtensionInfo` manifests.
2. **Dependency Resolution**: Dependencies are resolved in topological order. Missing or incompatible dependencies prevent loading.
3. **Assembly Loading**: Extension assemblies are loaded into the application context.
4. **Build Phase**: `BuildExtension(CyrenaBuilder)` is called on each `IExtension` implementation.
5. **Service Provider Build**: The DI container is built after all extensions have configured services.
6. **Run Phase**: `IStartupTask.RunAsync` is called for all registered startup tasks.

---

## Usage for Extension Developers

Reference `Cyrena.Extensa.Core` to:
1. Implement `IExtension` (or extend `Extension`) as the entry point
2. Define `ExtensionInfo` metadata for the extension manifest
3. Specify `Dependency` objects for required extensions
4. Use `CyrenaBuilder` in `BuildExtension` to register all services, modes, plugins, and UI components

**Example - Minimal Extension:**
```csharp
public class MyExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        builder.Services.AddSingleton<IMyService, MyService>();
    }
}
```

**Example - Extension with Dependencies:**
```csharp
// In ExtensionInfo (JSON manifest):
{
  "id": "com.example.advanced",
  "name": "Advanced Features",
  "version": "1.2.0",
  "dependencies": [
    { "id": "com.example.base", "minVersion": "1.0.0" }
  ]
}
```

**Required References for a Typical Extension:**
- `Cyrena.Core` - Core contracts and builders
- `Cyrena.Extensa.Core` - Extension entry point
- `Cyrena.Components.Core` - If adding UI components
- `Cyrena.Persistence.Core` - If using data storage
- Concrete persistence implementation (e.g., `Cyrena.Persistence.File`)
- `Microsoft.SemanticKernel` - For kernel plugins and functions

---

## Project Dependencies
- `Cyrena.Core` — `CyrenaBuilder`, `Entity`

---

## Package Information
- **PackageId**: `Cyrena.Extensa.Core`
- **Version**: `0.6.0`
- **Authors**: Vaya Nova
- **Description**: Core models, interfaces & services to develop extensions for Cyrena
- **Title**: Cyréna Extensa Core
- **Repository**: https://github.com/Christo262/Cyrena_App
- **ProjectUrl**: https://cyrena.dev
- **PostPack Target**: Copies NuGet package to `../../../sdk`
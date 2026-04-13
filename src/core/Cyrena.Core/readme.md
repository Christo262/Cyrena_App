# Cyrena.Core SDK

**Version:** 0.3.0  
**Target Framework:** .NET 10.0  
**Core Dependency:** Microsoft.SemanticKernel

Cyrena.Core is the core library for the Cyrena AI assistant application. It provides Semantic Kernel integration, chat management, LLM connection handling, and assistant mode configuration. **This library does not define application plugins** — it provides the underlying infrastructure that plugins consume.

---

## Architecture Overview

### Relationship Between Concepts

```
┌─────────────────────────────────────────────────────────────────┐
│                    Application Runtime                          │
│                                                                 │
│  ┌──────────────────┐     ┌─────────────────────────────────┐  │
│  │  Application     │     │       Cyrena.Core               │  │
│  │  Plugins         │     │                                 │  │
│  │  (loaded from    │────▶│  ┌───────────────────────────┐   │  │
│  │   other          │     │  │  IAssistantPlugin        │   │  │
│  │   assemblies)    │     │  │  (service in DI)         │   │  │
│  │                  │     │  │  Configures modes        │   │  │
│  └──────────────────┘     │  └───────────┬───────────────┘   │  │
│         │                  │              │                   │  │
│         │ Adds to DI:      │              │ Registers         │  │
│         │ • IAssistantPlugin│              │                   │  │
│         │ • IAssistantMode │◀─────────────┘                   │  │
│         │                  │                                  │  │
│         │                  │  ┌───────────────────────────┐   │  │
│         │                  │  │  IAssistantMode           │   │  │
│         │                  │  │  (kernel configuration)   │   │  │
│         │                  │  └───────────────────────────┘   │  │
│         │                  │                                  │  │
│         │                  │  ┌───────────────────────────┐   │  │
│         │                  │  │  IKernelController        │   │  │
│         │                  │  │  (manages per-chat        │   │  │
│         │                  │  │   kernel instances)       │   │  │
│         │                  │  └───────────────────────────┘   │  │
│         │                  │                                  │  │
│         │                  │  ┌───────────────────────────┐   │  │
│         │                  │  │  IChatMessageService      │   │  │
│         │                  │  │  (dual history model)     │   │  │
│         │                  │  └───────────────────────────┘   │  │
│         │                  │                                  │  │
│         │                  │  ┌───────────────────────────┐   │  │
│         │                  │  │  IConnection              │   │  │
│         │                  │  │  (LLM provider adapter)   │   │  │
│         │                  │  └───────────────────────────┘   │  │
│         │                  └─────────────────────────────────┘  │
│         │                                                       │
└─────────┴───────────────────────────────────────────────────────┘

Key Point: IAssistantPlugin is NOT the application plugin. It is a DI service
used to configure kernel behavior. Application plugins are loaded separately
from other assemblies and may register IAssistantPlugin or IAssistantMode
implementations into the DI container.
```

---

## Contracts

### Assistant Mode System

#### IAssistantMode
Defines behavior configuration for a Semantic Kernel instance. Modes represent different operational contexts (e.g., "default", "code-assist", "creative").

```csharp
public interface IAssistantMode
{
    // Unique identifier for this mode
    string Id { get; }
    
    // Configure kernel behavior for this mode
    Task ConfigureAsync(CyrenaKernelBuilder builder, CancellationToken cancellationToken = default);
    
    // Handle deletion
    Task DeleteAsync(CancellationToken cancellationToken = default);
    
    // Handle editing
    Task EditAsync(CancellationToken cancellationToken = default);
}

// Default mode identifier
const string IAssistantMode.Default = "assistant-default";
```

#### IAssistantPlugin
**NOT an application plugin.** A DI service that configures one or many `IAssistantMode` implementations into the kernel.

```csharp
public interface IAssistantPlugin : IAssistantMode
{
    // Which modes this plugin configures
    IReadOnlyList<string> Modes { get; }
    
    // Execution priority (lower = runs first)
    int Priority { get; }
    
    // Apply all modes to the kernel builder
    Task LoadAsync(CyrenaKernelBuilder builder, CancellationToken cancellationToken = default);
}
```

**Key Distinction:**
- **IAssistantMode** = A single kernel configuration (what to add to the kernel)
- **IAssistantPlugin** = A service in DI that registers one or more modes into the kernel

---

### Kernel Management

#### IKernelController
Manages all Semantic Kernel instances in the application. Each chat has its own kernel instance.

```csharp
public interface IKernelController
{
    // Plugin/Mode management
    Task LoadPluginAsync(string chatId, IAssistantPlugin plugin, CancellationToken cancellationToken = default);
    Task UnloadPluginAsync(string chatId, string pluginId, CancellationToken cancellationToken = default);
    
    // Kernel lifecycle
    Task<Kernel> CreateKernelAsync(string chatId, CancellationToken cancellationToken = default);
    Task UpdateKernelAsync(string chatId, CancellationToken cancellationToken = default);
    Task DeleteKernelAsync(string chatId, CancellationToken cancellationToken = default);
    
    // Event subscriptions
    EventPipe OnChatDelete { get; }
    EventPipe OnChatCreate { get; }
    EventPipe OnChatUpdate { get; }
    EventPipe OnChatUnload { get; }
}
```

#### IConnection
Connection to an LLM service provider. **Kernel-locked.**

```csharp
public interface IConnection
{
    // Send message and get response
    Task<AuthorRole> HandleAsync(ChatMessageContent input, KernelFunctionMetadata[] tools, CancellationToken cancellationToken = default);
    
    // Stream response tokens
    IAsyncEnumerable<StreamingChatMessageContent> HandleStreamingAsync(ChatMessageContent input, KernelFunctionMetadata[] tools, CancellationToken cancellationToken = default);
    
    // Get role for returned content
    AuthorRole GetRole();
    
    // Check if message should go to kernel
    bool GoesToKernel(KernelContent content);
}
```

#### IConnectionProvider
Provides available LLM connections.

```csharp
public interface IConnectionProvider
{
    // List all available connections
    Task<IList<ConnectionInfo>> ListConnectionsAsync(CancellationToken cancellationToken = default);
    
    // Check if connection exists
    Task<bool> HasConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
    
    // Get connection info
    Task<ConnectionInfo> AttachAsync(string connectionId, CancellationToken cancellationToken = default);
}
```

---

### Chat & History Management

#### IChatMessageService
Maintains chat history with dual history model. **Kernel-locked.**

```csharp
public interface IChatMessageService
{
    // Current configuration
    ChatConfiguration Configuration { get; }
    
    // Kernel-visible history (all messages including internal)
    IList<ChatMessageContent> KernelHistory { get; }
    
    // User-visible history (display-only messages)
    IList<ChatMessageContent> DisplayHistory { get; }
    
    // Current streaming tokens
    IList<StreamingChatMessageContent> StreamingTokens { get; }
    
    // Load history from storage
    Task LoadHistoryAsync(string chatId, CancellationToken cancellationToken = default);
    
    // Add message to history
    Task AddMessageAsync(ChatMessageContent message, CancellationToken cancellationToken = default);
    
    // Get messages for kernel
    IList<ChatMessageContent> GetMessages();
    
    // Clear current history
    Task ClearAsync(CancellationToken cancellationToken = default);
}
```

#### IIterationService
Manages inference iterations (user message → model response). **Kernel-locked.**

```csharp
public interface IIterationService
{
    bool InInferring { get; }
    
    // Start/end inference cycle
    void StartInferring();
    void EndInferring();
    
    // Callback hooks
    Action OnIterationStart { get; set; }
    Action<AuthorRole, bool> OnIterationEnd { get; set; }
}
```

#### IChatConfigurationService
Access to current chat configuration.

```csharp
public interface IChatConfigurationService
{
    ChatConfiguration Current { get; }
    
    // Save configuration
    Task SaveAsync(ChatConfiguration configuration, CancellationToken cancellationToken = default);
}
```

---

### Settings & Dialogs

#### ISettingsService
Generic key-value persistence for plugins.

```csharp
public interface ISettingsService
{
    // Save value to settings
    Task SaveAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    
    // Read value from settings
    Task<T?> ReadAsync<T>(string key, CancellationToken cancellationToken = default);
}
```

#### IFileDialog
Abstracted file dialog interface.

```csharp
public interface IFileDialog
{
    // Open file dialog
    Task<string?> OpenAsync(string? filter = null, string? title = null);
    
    // Save file dialog
    Task<string?> SaveAsync(string? filter = null, string? title = null);
    
    // Folder picker
    Task<string?> FolderAsync(string? title = null);
}
```

---

### Startup System

#### IStartupTask
Tasks that execute after `IServiceProvider` is built.

```csharp
public interface IStartupTask
{
    int Order { get; }
    
    Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
```

---

## Models

### ChatConfiguration
Saved chat configuration with mode and connection selection.

```csharp
public class ChatConfiguration : Entity
{
    public string Title { get; set; }
    public string AssistantModeId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}
```

### CyrenaKernelBuilder
Wrapper around `IKernelBuilder` for mode configuration.

```csharp
public class CyrenaKernelBuilder
{
    public IKernelBuilder KernelBuilder { get; }
    public ChatConfiguration Configuration { get; }
    public IDictionary<string, FeatureOptions> FeatureOptions { get; }
}
```

### ConnectionInfo
Record containing connection details.

```csharp
public record ConnectionInfo(
    string Id,
    string Name,
    string Source,
    string ModelId,
    IConnectionProvider Provider
);
```

### AdditionalMessageContent
Extended message metadata for kernel content.

```csharp
public class AdditionalMessageContent
{
    public KernelContent Content { get; set; }
    public string Name { get; set; }
}
```

### InfoMessageContentItem
Display-only kernel content for showing informational items (e.g., file names).

```csharp
public class InfoMessageContentItem : KernelContent
{
    public InfoMessageContentItem(string text, string? metadata = null);
}
```

### ToolResult<T>
Generic result wrapper for tool operations.

```csharp
public class ToolResult<T> : IJsonSerializable
{
    public bool Success { get; }
    public string Message { get; }
    public T? Result { get; }
}
```

### EventPipeline / EventPipe
Event subscription system with automatic cleanup.

```csharp
public class EventPipeline
{
    public EventPipe CreatePipe();
    public EventPipe<T> CreatePipe<T>();
}

public class EventPipe
{
    // Invoke all handlers
    public virtual void Invoke();
    
    // Remove handler after first invoke
    public virtual IDisposable Subscribe(Action handler);
}

public class EventPipe<T> : IDisposable
{
    public virtual void Invoke(T args);
    public virtual IDisposable Subscribe(Action<T> handler);
}
```

### Entity / IEntity
Base class with identifier.

```csharp
public interface IEntity
{
    string Id { get; }
}

public class Entity : IEntity
{
    public string Id { get; set; }
}
```

---

## Options

### ChatOptions
Configuration for chat message handling and role mappings.

```csharp
public class ChatOptions
{
    // Author role mappings
    public AuthorRole SystemRole { get; set; }
    public AuthorRole AssistantRole { get; set; }
    public AuthorRole UserRole { get; set; }
    public AuthorRole ToolRole { get; set; }
    
    // Logging role mappings
    public AuthorRole LogInfoRole { get; set; }
    public AuthorRole LogSuccessRole { get; set; }
    public AuthorRole LogWarnRole { get; set; }
    public AuthorRole LogErrorRole { get; set; }
    
    // Behavior flags
    public bool IncludeLogsInDisplay { get; set; }
    public bool AutoSave { get; set; }
}
```

### CyrenaBuilder
Main application builder for registering modes and plugins.

```csharp
public class CyrenaBuilder
{
    // Feature assemblies for plugin discovery
    public IList<Assembly> FeatureAssemblies { get; }
    
    // Feature options per feature ID
    public IDictionary<string, FeatureOptions> FeatureOptions { get; }
    
    // Lifecycle hooks
    public IList<Action<IServiceProvider>> BuildActions { get; }
    public IList<Func<IServiceProvider, CancellationToken, Task>> RunActions { get; }
}
```

### FeatureOptions
Empty marker class for typed feature options dictionary.

```csharp
public class FeatureOptions { }
```

---

## Extension Methods

### CyrenaBuilderExtensions
Application-level registration.

```csharp
public static class CyrenaBuilderExtensions
{
    // Add feature options
    public static CyrenaBuilder AddFeatureOptions(this CyrenaBuilder builder, string featureId, FeatureOptions options);
    
    // Add feature assembly for discovery
    public static CyrenaBuilder AddFeatureAssembly(this CyrenaBuilder builder, Assembly assembly);
    
    // Add startup task
    public static CyrenaBuilder AddStartupTask<T>(this CyrenaBuilder builder) where T : class, IStartupTask;
    
    // Add assistant mode
    public static CyrenaBuilder AddAssistantMode<T>(this CyrenaBuilder builder, string modeId) where T : class, IAssistantMode;
    
    // Add plugin (registers IAssistantPlugin as DI service)
    public static CyrenaBuilder AddAssistantPlugin<T>(this CyrenaBuilder builder, string modeId) where T : class, IAssistantPlugin;
}
```

### CyrenaKernelBuilderExtensions
Kernel-level feature configuration.

```csharp
public static class CyrenaKernelBuilderExtensions
{
    public static CyrenaKernelBuilder AddFeatureOptions(this CyrenaKernelBuilder builder, string featureId, FeatureOptions options);
}
```

### KernelBuilderExtensions
Semantic Kernel setup helpers.

```csharp
public static class KernelBuilderExtensions
{
    public static IKernelBuilder AddStartupTask<T>(this IKernelBuilder builder) where T : class, IStartupTask;
    
    public static IKernelBuilder AddSystemPrompt(this IKernelBuilder builder, string prompt);
}
```

### ChatMessageServiceExtensions
Chat message helpers.

```csharp
public static class ChatMessageServiceExtensions
{
    // Logging helpers
    public static Task LogInfoAsync(this IChatMessageService service, string message);
    public static Task LogWarnAsync(this IChatMessageService service, string message);
    public static Task LogErrorAsync(this IChatMessageService service, string message);
    public static Task LogSuccessAsync(this IChatMessageService service, string message);
    
    // Add message helpers
    public static Task AddSystemMessageAsync(this IChatMessageService service, string content);
    public static Task AddUserMessageAsync(this IChatMessageService service, string content);
    public static Task AddAssistantMessageAsync(this IChatMessageService service, string content);
    public static Task AddToolMessageAsync(this IChatMessageService service, string content, string? toolName = null);
}
```

### ChatOptionsExtensions
Determines message routing.

```csharp
public static class ChatOptionsExtensions
{
    // Returns true if content is display-only (not sent to kernel)
    public static bool IsDisplayContent(this ChatOptions options, KernelContent content);
    
    // Returns true if content goes to kernel
    public static bool IsKernelContent(this ChatOptions options, KernelContent content);
}
```

### Resources
Reads embedded resources from assemblies.

```csharp
public static class Resources
{
    public static string Read(this Assembly assembly, string resourceName);
}
```

---

## Architecture Patterns

### Kernel Locking
Several services (`IConnection`, `IChatMessageService`, `IIterationService`) are **kernel-locked**. Operations require holding the kernel context and should not be called concurrently with other kernel operations.

### Dual History Model
`IChatMessageService` maintains two separate histories:
- **KernelHistory**: All messages including tool calls and internal content
- **DisplayHistory**: Only user-visible messages (excludes system logs, tool calls, etc.)

### Feature Options Pattern
Configuration is passed to modes/plugins via the `FeatureOptions` dictionary at both application level (`CyrenaBuilder`) and kernel level (`CyrenaKernelBuilder`).

### Event Pipeline
Custom event system with automatic handler disposal. Handlers are removed after first invoke or on error.

### Plugin Registration Flow
Application plugins (loaded from external assemblies) can register:
1. `IAssistantPlugin` - A DI service that configures one or more modes
2. `IAssistantMode` - Direct mode registrations

These are consumed by `IKernelController` when creating kernel instances for each chat.

---

## Usage Example

### Registering a Plugin Service (in application startup)

```csharp
// Register IAssistantPlugin as a DI service
builder.AddAssistantPlugin<MyAssistantPlugin>("my-mode");

// In MyAssistantPlugin implementation
public class MyAssistantPlugin : IAssistantPlugin
{
    public IReadOnlyList<string> Modes => ["my-mode"];
    public int Priority => 100;
    
    public async Task LoadAsync(CyrenaKernelBuilder builder, CancellationToken ct)
    {
        // Add services to kernel
        builder.KernelBuilder.Services.AddSingleton<IMyService, MyService>();
        
        // Configure with feature options
        if (builder.FeatureOptions.TryGetValue("my-feature", out var options))
        {
            // Apply feature configuration
        }
    }
}
```

### Registering a Direct Mode (alternative)

```csharp
// Register IAssistantMode directly
builder.AddAssistantMode<MyAssistantMode>("my-mode");

public class MyAssistantMode : IAssistantMode
{
    public string Id => "my-mode";
    
    public Task ConfigureAsync(CyrenaKernelBuilder builder, CancellationToken ct)
    {
        builder.KernelBuilder.Services.AddSingleton<IMyService, MyService>();
        return Task.CompletedTask;
    }
}
```

---

## Package Dependencies

- `Microsoft.SemanticKernel` (Semantic Kernel abstractions)
- `Microsoft.Extensions.Configuration` (Configuration support)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (DI abstractions)
- `Microsoft.Extensions.Options` (Options pattern)
- `Microsoft.Extensions.Options.ConfigurationExtensions` (Options from config)
- `Newtonsoft.Json` (JSON serialization)

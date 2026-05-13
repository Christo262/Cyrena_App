# Cyrena.Core SDK

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

## Overview

`Cyrena.Core` is the foundational class library of the Cyréna AI assistant framework. It defines all core contracts, data models, application builders, and extension methods that the rest of the solution depends on. Any extension or plugin for Cyréna must reference this package.

**Namespace:** `Cyrena` (contracts, models, options, extensions)
**Target Framework:** .NET 10.0

---

## Contracts

### `IAssistantMode`
Configures behavior surrounding the `Microsoft.SemanticKernel.Kernel` for a specific assistant mode.

```csharp
public interface IAssistantMode
{
    public const string AssistantModeDefault = "assistant-default";
    string Id { get; }
    Task ConfigureAsync(CyrenaKernelBuilder builder);
    Task DeleteAsync(ChatConfiguration config);
    Task EditAsync(ChatConfiguration config, IServiceProvider services);
}
```

- `Id`: Unique identifier for the mode.
- `ConfigureAsync`: Called when a kernel is loaded for this mode. Use to add plugins, prompts, and services.
- `DeleteAsync`: Called when a chat using this mode is deleted.
- `EditAsync`: Called when a chat configuration is edited.

### `IAssistantPlugin`
Adds additional services, features, or functions to an `IAssistantMode`.

```csharp
public interface IAssistantPlugin
{
    string Id { get; }
    string[] Modes { get; }      // Empty = applicable to all modes
    int Priority { get; }
    bool Required { get; }       // false = can be deactivated
    string Title { get; }
    Task LoadAsync(CyrenaKernelBuilder builder);
}
```

### `IChatMessageService` (Kernel-locked)
Maintains chat history with separate kernel and display histories.

```csharp
public interface IChatMessageService : IDisposable
{
    IReadOnlyList<ChatMessageContent> KernelHistory { get; }
    IReadOnlyList<ChatMessageContent> DisplayHistory { get; }
    ChatOptions Options { get; }
    
    IDisposable OnStreamToken(Action<string?> callback);
    IDisposable OnDisplayHistoryChanged(Action<ChatHistory> callback);
    IDisposable OnKernelHistoryChanged(Action<ChatHistory> callback);
    IDisposable OnHistoryLoaded(Action<ChatHistory> callback);
    Task<ChatHistory> GetKernelHistory();
    
    void LoadHistory(IEnumerable<ChatMessageContent> kernelHistory, IEnumerable<ChatMessageContent>? displayHistory);
    Task LoadHistoryAsync();
    Task AddMessage(ChatMessageContent content);
    Task AddMessage(AuthorRole role, string? content);
    Task AddMessage(AuthorRole role, string? input, params AdditionalMessageContent[] items);
    Task ClearHistoryAsync();
    void Stream(string? token);
}
```

### `IChatConfigurationService`
Provides access to the current chat's persistent configuration.

```csharp
public interface IChatConfigurationService
{
    ChatConfiguration Config { get; }
    Task SaveConfigurationAsync();
}
```

### `IConnection` (Kernel-locked)
Connection to an LLM service provider.

```csharp
public interface IConnection
{
    Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default);
    Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items);
    void FunctionCallStart();
}
```

### `IConnectionProvider`
Manages available LLM connections and attaches them to kernels.

```csharp
public interface IConnectionProvider
{
    Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync();
    Task<bool> HasConnectionAsync(string id);
    Task<ConnectionInfo> AttachAsync(IKernelBuilder builder, string connectionId);
}
```

### `IConversationHistoryTransformer` (Kernel-locked)
Used to modify the conversation history to ensure context is short.

```csharp
public interface IConversationHistoryTransformer
{
    Task ApplyPostStreamModification(ChatHistory history);
    Task<ChatHistory> TransformPreIterationHistory(ChatHistory history);
}

public abstract class ConversationHistoryTransformer : IConversationHistoryTransformer
{
    public virtual Task ApplyPostStreamModification(ChatHistory history) => Task.CompletedTask;
    public virtual Task<ChatHistory> TransformPreIterationHistory(ChatHistory history) => Task.FromResult(history);
}
```

### `IFileHandler` (Kernel-locked)
Handles file attachments in chat messages. Determines supported file types and converts file data into `AdditionalMessageContent`.

```csharp
public interface IFileHandler
{
    bool HandlesType(string contentType, string fileName);
    Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name);
    Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name);
    Task<KernelContent?> GetKernelContent(Stream data, string contentType, string name);
    Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name);
    string[] GetSupportedMimeTypes();
    Dictionary<string, string> GetExtensionMimeTypeMapping();
}
```

### `IFileHandlerFactory` (Kernel-locked)
Provides easier access to all `IFileHandler` instances in a `Kernel`.

```csharp
public interface IFileHandlerFactory
{
    bool HasFileHandlers { get; }
    bool CanHandleType(string contentType, string fileName);
    string[] GetSupportedMimeTypes();
    Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name);
    Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name);
    Task<KernelContent?> GetKernelContent(Stream data, string contentType, string name);
    Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name);
    string? GetExtension(string mimeType);
}
```

### `IKernelController`
Manages all `Kernel` instances, loading, creating, updating, and unloading them.

```csharp
public interface IKernelController : IDisposable
{
    Task<Kernel> LoadAsync(ChatConfiguration config);
    Task<Kernel> LoadAsync(string id);
    Task Delete(ChatConfiguration config);
    Task<Kernel> Create(ChatConfiguration config);
    Task UpdateAsync(ChatConfiguration config, bool reload = false);
    Kernel? GetKernel(string id);
    bool KernelActive(string id);
    void Unload(ChatConfiguration config);
    
    IDisposable OnChatDelete(Action<ChatConfiguration> cb);
    IDisposable OnChatCreate(Action<ChatConfiguration> cb);
    IDisposable OnChatUpdate(Action<ChatConfiguration> cb);
    IDisposable OnChatUnload(Action<ChatConfiguration> cb);
    IDisposable OnChatLoaded(Action<ChatConfiguration> cb);
}
```

### `IPromptManager` (Kernel-locked)
Dynamic system prompt configuration.

```csharp
public interface IPromptManager
{
    IReadOnlyList<Prompt> Prompts { get; }
    string AddPrompt(int order, string content);
    void UpdatePrompt(string id, string content);
    void RemovePrompt(string id);
    [Obsolete("Use IConversationHistoryTransformer instead")]
    Func<ChatHistory, ChatOptions, IEnumerable<ChatMessageContent>>? ModifyKernelHistoryFunc { get; set; }
}
```

### `IIterationService` (Kernel-locked)
Manages a single chat iteration from user input to model completion.

```csharp
public interface IIterationService : IDisposable
{
    string? Input { get; set; }
    bool Inferring { get; }
    string? IterationId { get; }
    bool IsPaused { get; }
    int QueueCount { get; }
    bool IsPausedByAi { get; }
    IReadOnlyList<QueuedInput> Queued { get; }
    
    void InferenceStart();
    void InferenceEnd();
    IDisposable OnIterationStart(Action<bool> callback);
    IDisposable OnIterationEnd(Action<bool> callback);
    void Iterate(AuthorRole role, Kernel kernel, params AdditionalMessageContent[]? items);
    void Cancel();
    void PauseQueue(bool by_ai = false);
    void ContinueQueue();
    void CancelInput(string id);
}
```

### `ISettingsService`
Encrypted settings storage.

```csharp
public interface ISettingsService
{
    void Save<T>(string key, T value) where T : class;
    T? Read<T>(string key) where T : class;
}
```

### `IFileDialog`
Cross-platform file dialog abstraction.

```csharp
public interface IFileDialog
{
    Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? ftr);
    Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? ftr, string? defaultPath = null);
    void ExploreFolder(string folderPath);
    Task<string?> SelectFolder(string title = "Select Folder", string? current = null);
}
```

### `IStartupTask`
Post-DI-build startup task with ordered execution.

```csharp
public interface IStartupTask
{
    int Order { get; }
    Task RunAsync(CancellationToken cancellationToken = default);
}
```

---

## Models

### `ChatConfiguration` (extends `Entity`)
Persistent configuration for each chat conversation.

```csharp
public sealed class ChatConfiguration : Entity
{
    public const string Icon = "icon";
    public const string Group = "group";
    
    public string? this[string key] { get; set; }
    public string? Title { get; set; }
    public string AssistantModeId { get; set; } = default!;
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    [Required]
    public string ConnectionId { get; set; } = default!;
    public Dictionary<string, string?> Properties { get; set; }
    public List<string> PluginIds { get; set; }
    public string? WorkingDirectory { get; set; }
}
```

### `CyrenaKernelBuilder`
Per-chat kernel configuration builder passed to modes and plugins.

```csharp
public sealed class CyrenaKernelBuilder
{
    public CyrenaKernelBuilder(ChatConfiguration chatConfiguration, IKernelBuilder kernelBuilder);
    public ChatConfiguration ChatConfiguration { get; }
    public IKernelBuilder KernelBuilder { get; }
    public IDictionary<string, object> FeatureOptions { get; }
    public IServiceCollection Services => KernelBuilder.Services;
    public IKernelBuilderPlugins Plugins => KernelBuilder.Plugins;
}
```

### `CyrenaBuilder`
Application-level DI builder.

```csharp
public sealed class CyrenaBuilder
{
    public static string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
    public static string UserContentDirectory = Path.Combine(AppDataDirectory, "public");
    
    public IServiceCollection Services { get; }
    public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
    public IDictionary<string, object> FeatureOptions { get; }
    public IList<Action<CyrenaBuilder>> BuildActions { get; }
    public IList<Action<IServiceProvider, CancellationToken>> RunActions { get; }
    
    public void AddBuildAction(Action<CyrenaBuilder> action);
    public void AddRunAction(Action<IServiceProvider, CancellationToken> action);
    public CancellationToken GetLifetimeCT();
    public void CancelLifetimeCT();
    public void Build();
}
```

### `CyrenaOptions`
Holds feature assemblies for runtime lookup.

```csharp
public sealed class CyrenaOptions
{
    public CyrenaOptions(IDictionary<string, IList<Assembly>> featureAssemblies);
    public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
}
```

### `EventPipeline`
Custom publish/subscribe event system with automatic cleanup.

```csharp
public abstract class EventPipeline : IDisposable
{
    protected void InvokePipeline(string key);
    protected void InvokePipeline<T>(string key, T value);
    protected IDisposable ConfigurePipe(string key, Action cb);
    protected IDisposable ConfigurePipe<T>(string key, Action<T> cb);
}
```

### `Entity` / `IEntity`
Base entity with `Id`.

```csharp
public interface IEntity { string Id { get; set; } }
public abstract class Entity : IEntity { public virtual string Id { get; set; } = default!; }
```

### `AdditionalMessageContent`
Named wrapper for `KernelContent`.

```csharp
public class AdditionalMessageContent
{
    public AdditionalMessageContent(string name, KernelContent item);
    public string Name { get; set; }
    public KernelContent Item { get; set; }
}
```

### `ConnectionInfo`
LLM connection metadata.

```csharp
public record ConnectionInfo(string Id, string Name, string Source, string ModelId, IConnectionProvider Provider, bool SupportImages, bool SupportFiles);
```

### `Prompt`
System prompt with ordering.

```csharp
public sealed class Prompt : Entity
{
    public Prompt();
    public int Order { get; init; }
    public string Content { get; init; } = default!;
}
```

### `QueuedInput`
Represents a single queued chat input message.

```csharp
public sealed class QueuedInput
{
    public QueuedInput(AuthorRole role, string? content, AdditionalMessageContent[]? items);
    public string Id { get; }
    public AuthorRole Role { get; }
    public string Content { get; set; }
    public List<AdditionalMessageContent> Items { get; set; }
}
```

### `ToolResult` / `ToolResult<T>`
Function call result wrapper.

```csharp
public class ToolResult : IJsonSerializable
{
    public ToolResult();
    public ToolResult(bool success, string? message);
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string ToJson();
}

public class ToolResult<T> : ToolResult where T : class
{
    public ToolResult();
    public ToolResult(bool success, string? message) : base(success, message);
    public ToolResult(T result, bool success = true, string? message = null) : base(success, message);
    public T? Result { get; set; }
}
```

### `InfoMessageContentItem`
Display-only content item extending `KernelContent`.

```csharp
public sealed class InfoMessageContentItem : KernelContent
{
    public InfoMessageContentItem();
    public InfoMessageContentItem(string fileName);
    public string FileName { get; set; } = default!;
}
```

### `IJsonSerializable` / `JsonStringObject`
JSON serialization marker interface.

```csharp
public interface IJsonSerializable { string ToJson(); }
[Obsolete]
public abstract class JsonStringObject : IJsonSerializable { public string ToJson(); }
```

---

## Options

### `ChatOptions`
Role configuration for chat messages.

```csharp
public sealed class ChatOptions
{
    public AuthorRole System { get; }
    public AuthorRole Assistant { get; }
    public AuthorRole User { get; }
    public AuthorRole Tool { get; }
    public AuthorRole LogInfo { get; }
    public AuthorRole LogSuccess { get; }
    public AuthorRole LogWarn { get; }
    public AuthorRole LogError { get; }
    public bool IncludeLogsInDisplay { get; set; } = true;
    public AuthorRole[] MessagePersistRoles { get; set; }  // Defaults to [Assistant, User, Tool]
}
```

---

## Extension Methods

### `CyrenaBuilderExtensions`

```csharp
public static class CyrenaBuilderExtensions
{
    public static void AddFeatureOption<T>(this CyrenaBuilder builder, T option) where T : class;
    public static object? GetFeatureOption(this CyrenaBuilder builder, string name);
    public static T GetFeatureOption<T>(this CyrenaBuilder builder) where T : class;
    public static void AddFeatureAssembly(this CyrenaBuilder builder, string key, Assembly assembly);
    public static void AddFeatureAssembly<T>(this CyrenaBuilder builder, string key);
    public static IList<Assembly> GetFeatureAssemblies(this CyrenaOptions builder, string key);
    public static void AddStartupTask<TStartupTask>(this CyrenaBuilder builder) where TStartupTask : class, IStartupTask;
    public static void AddAssistantMode<TAssistantMode>(this CyrenaBuilder builder) where TAssistantMode : class, IAssistantMode;
    public static void AddAssistantPlugin<TAssistantPlugin>(this CyrenaBuilder builder) where TAssistantPlugin : class, IAssistantPlugin;
}
```

### `CyrenaKernelBuilderExtensions`

```csharp
public static class CyrenaKernelBuilderExtensions
{
    public static void AddFeatureOption<T>(this CyrenaKernelBuilder builder, T option) where T : class;
    public static object? GetFeatureOption(this CyrenaKernelBuilder builder, string name);
    public static T GetFeatureOption<T>(this CyrenaKernelBuilder builder) where T : class;
}
```

### `ChatMessageServiceExtensions`

```csharp
public static class ChatMessageServiceExtensions
{
    public static Task LogInfo(this IChatMessageService service, string? message);
    public static Task LogSuccess(this IChatMessageService service, string? message);
    public static Task LogWarn(this IChatMessageService service, string? message);
    public static Task LogError(this IChatMessageService service, string? message);
    public static Task AddSystemMessage(this IChatMessageService service, string? message);
    public static Task AddAssistantMessage(this IChatMessageService service, string? message);
    public static Task AddUserMessage(this IChatMessageService service, string? message);
    public static Task AddToolMessage(this IChatMessageService service, string? message);
}
```

### `ChatOptionsExtensions`

```csharp
public static class ChatOptionsExtensions
{
    public static bool IsDisplayContent(this ChatOptions options, ChatMessageContent content);
    public static bool IsKernelContent(this ChatOptions options, ChatMessageContent content);
}
```

### `KernelBuilderExtensions`

```csharp
public static class KernelBuilderExtensions
{
    public static void AddStartupTask<TStartupTask>(this IKernelBuilder builder) where TStartupTask : class, IStartupTask;
}
```

### `Resources`

```csharp
public static class Resources
{
    public static string Read(Assembly assembly, string resourceName);
}
```

---

## Usage for Extension Developers

Reference `Cyrena.Core` to:
1. Implement `IAssistantMode` or `IAssistantPlugin`
2. Access chat services like `IChatMessageService`, `IPromptManager`, `IIterationService`
3. Use `CyrenaBuilder` and `CyrenaKernelBuilder` to configure DI and kernel
4. Use `ChatConfiguration` and `Entity` for data models
5. Use `EventPipeline` for custom events
6. Use `ToolResult` for function call results
7. Use `IFileHandler` and `IFileHandlerFactory` for file attachment support
8. Use `IConversationHistoryTransformer` for history modification
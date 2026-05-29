## Overview

`Cyrena.Core` is the foundational class library of the Cyréna AI assistant framework. It defines all core contracts, data models, application builders, and extension methods that the rest of the solution depends on. Any extension or plugin for Cyréna must reference this package.

**Namespace:** `Cyrena` (contracts, models, options, extensions)
**Project Type:** Class Library
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
Connection to an LLM service provider. Receives a `ChatMessageContent` to process.

```csharp
public interface IConnection
{
    Task HandleAsync(ChatMessageContent content, CancellationToken ct = default);
    void FunctionCallStart();
}
```

- `HandleAsync`: Processes a chat message content (sends to LLM, handles response).
- `FunctionCallStart`: Called by function invocation filter to signal tool call start, helping suppress "thinking" messages.

### `IConnectionProvider`
Provides connections to LLM backends.

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

### `ICyrenaFileExporter` (Kernel-locked)
Exports conversation files as a `.cyrena` zipped archive with manifest info.

```csharp
public interface ICyrenaFileExporter
{
    Task<CyrenaFileManifest> ExportFilesAsync(string extensionId, Version extensionVersion, string importerId, Dictionary<string, string?> properties, string outPath, CancellationToken cancellationToken = default);
}
```

### `ICyrenaFileImporter` (Global service)
Handles processing of imported `.cyrena` files based on manifest. Must be registered as a global singleton.

```csharp
public interface ICyrenaFileImporter
{
    string Id { get; }
    Task ImportAsync(CyrenaFileManifest manifest, string absoluteDataPath, CancellationToken cancellationToken = default);
}
```

### `IFileHandler` (Kernel-locked)
Handles file attachments in chat messages.

```csharp
public interface IFileHandler
{
    bool HandlesType(string contentType, string fileName);
    Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null);
    string[] GetSupportedMimeTypes();
    Dictionary<string, string> GetExtensionMimeTypeMapping();
}
```

### `IFileHandlerFactory` (Kernel-locked)
Provides easier access to all `IFileHandler` instances in a `Kernel`. Also manages file attachments persistence.

```csharp
public interface IFileHandlerFactory
{
    bool HasFileHandlers { get; }
    bool CanHandleType(string contentType, string fileName);
    string[] GetSupportedMimeTypes();
    Task<KernelContent> GetKernelContent(string fileId, CancellationToken cancellationToken = default);
    Task<KernelContent?> SaveAsync(Stream data, string contentType, string name, CancellationToken cancellationToken = default);
    Task<KernelContent?> SaveAsync(byte[] data, string contentType, string name, CancellationToken cancellationToken = default);
    Task CancelAsync(KernelContent item, CancellationToken cancellationToken = default);
    string? GetExtension(string mimeType);
    Task<IEnumerable<FileAttachment>> ListAttachmentsAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetFileDataAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteFileAttachmentAsync(string id, CancellationToken cancellationToken = default);
    Task<FileAttachment> CreateAsync(string name, string contentType, byte[] content, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileAttachment att, CancellationToken cancellationToken = default);
    Task<FileAttachment?> GetAttachmentAsync(string id, CancellationToken cancellationToken = default);
}
```

### `IKernelController`
Manages all `Kernel` instances. Creates, loads, updates, and kills kernels when needed.

```csharp
public interface IKernelController : IDisposable
{
    IReadOnlyList<Kernel> ActiveKernels { get; }
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
    IDisposable OnChatLoadStart(Action<ChatConfiguration> cb);
    IDisposable OnChatLoaded(Action<ChatConfiguration> cb);
    IDisposable OnChatLoadError(Action<Exception> cb);
}
```

### `IKernelResolver` (Kernel-locked)
Provides access to the current kernel instance via a factory function.

```csharp
public interface IKernelResolver
{
    Func<Kernel> Resolve { get; }
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
}
```

### `IIterationService` (Kernel-locked)
Manages a single chat iteration from user input to model completion.

```csharp
public interface IIterationService : IDisposable
{
    ChatMessageContent? Input { get; set; }
    bool Inferring { get; }
    Ulid? IterationId { get; }
    bool IsPaused { get; }
    int QueueCount { get; }
    bool IsPausedByAi { get; }
    IReadOnlyList<QueuedInput> Queued { get; }
    
    void InferenceStart();
    void InferenceEnd();
    IDisposable OnIterationStart(Action<bool> callback);
    IDisposable OnIterationEnd(Action<bool> callback);
    void Iterate();
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
    Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? filter);
    Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? filter, string? defaultPath = null);
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
    public string FileStoragePath { get; set; }
    public HistoryInclusionMode HistoryInclusion { get; set; } = HistoryInclusionMode.All;
}
```

### `ChatMessageContentEntity` (extends `Entity`)
Used to persist chat history to storage.

```csharp
public sealed class ChatMessageContentEntity : Entity
{
    public ChatMessageContentEntity();
    public ChatMessageContentEntity(ChatMessageContent content, Ulid? iterationId = null);
    
    public Ulid? IterationId { get; set; }
    public DateTime Date { get; set; }
    public string? Role { get; set; }
    public List<KernelContentEntity> Items { get; set; }
    public string? ModelId { get; set; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; set; }
    public string? MimeType { get; set; }
    
    public ChatMessageContent AsChatMessage();
}

public sealed class KernelContentEntity : Entity
{
    public KernelContentEntity();
    public KernelContentEntity(KernelContent item);
    
    public KernelContent? Item { get; set; }
    public bool IsContentType<TKernelContent>() where TKernelContent : KernelContent;
}
```

### `CyrenaFileManifest`
Represents the manifest inside a `.cyrena` zipped archive.

```csharp
public sealed class CyrenaFileManifest
{
    public string Extension { get; set; } = default!;           // JsonPropertyName: "extension.required"
    public Version Version { get; set; } = default!;          // JsonPropertyName: "required.extension.version.min"
    public string ImporterId { get; set; } = default!;        // JsonPropertyName: "importer.id"
    public Dictionary<string, string?> Properties { get; set; } // JsonPropertyName: "cyrena.properties"
    
    public string? this[string key] { get; set; }
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
    public static readonly string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
    public static readonly string UserContentDirectory = Path.Combine(AppDataDirectory, "public");
    public static readonly string ConversationsData = Path.Combine(AppDataDirectory, "conversations");
    
    public IServiceCollection Services { get; }
    public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
    public IDictionary<string, object> FeatureOptions { get; }
    public IList<Action<CyrenaBuilder>> BuildActions { get; }
    public IList<Action<IServiceProvider, CancellationToken>> RunActions { get; }
    
    public void AddBuildAction(Action<CyrenaBuilder> action);
    public void AddRunAction(Action<IServiceProvider, CancellationToken> action);
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

### `ConnectionInfo`
LLM connection metadata.

```csharp
public record ConnectionInfo(string Id, string Name, string Source, string ModelId, IConnectionProvider Provider, bool SupportImages, bool SupportFiles);
```

### `FileAttachment` (extends `Entity`)
Represents a persisted file attachment.

```csharp
public sealed class FileAttachment : Entity
{
    public string MimeType { get; set; } = default!;
    public string Path { get; set; } = default!;
    public string InternalName { get; set; } = default!;
    public List<string> Tools { get; set; }
    public Dictionary<string, string?> Properties { get; set; }
    public string? this[string key] { get; set; }
    
    public static FileAttachment From(string file_name, string content_type, string path, string original_name, params string[] tools);
    public FileReferenceContent ToFileReference();
}
```

### `HistoryInclusionMode`
Controls how much chat history is sent to the AI.

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HistoryInclusionMode
{
    All,        // Includes entire history
    LastTwo,    // Includes only last 2 iterations
    LastTen,    // Includes only last 10 iterations
    Instruct    // Includes no history, instruct mode
}
```

### `Prompt`
System prompt with ordering.

```csharp
public sealed class Prompt
{
    public Prompt();
    public string Id { get; init; }
    public int Order { get; init; }
    public string Content { get; init; } = default!;
}
```

### `QueuedInput`
Represents a single queued chat input message.

```csharp
public sealed class QueuedInput
{
    public QueuedInput(ChatMessageContent message);
    public string Id { get; }
    public AuthorRole Role => Message.Role;
    public ChatMessageContent Message { get; set; }
}
```

### `ToolResult` / `ToolResult<T>`
Function call result wrapper.

```csharp
public class ToolResult
{
    public ToolResult();
    public ToolResult(bool success, string? message);
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ToolResult<T> : ToolResult, ISuppressibleResult where T : class
{
    public ToolResult();
    public ToolResult(bool success, string? message) : base(success, message);
    public ToolResult(T result, bool success = true, string? message = null) : base(success, message);
    public T? Result { get; set; }
    public string Suppress();
}
```

### `ISuppressibleResult`
Interface for suppressing function results to reduce context size.

```csharp
public interface ISuppressibleResult
{
    string Suppress();
}
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
    public static IList<Assembly> GetFeatureAssemblies(this CyrenaOptions options, string key);
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
    public static Task AddMessage(this IChatMessageService service, AuthorRole role, string? content);
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

### `KernelContentExtensions`

```csharp
public static class KernelContentExtensions
{
    public static TextContent ToTextContent(this FileReferenceContent reference);
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
9. Use `ISuppressibleResult` to reduce context size after tool calls
10. Use `ICyrenaFileExporter` and `ICyrenaFileImporter` for custom file import/export
11. Use `HistoryInclusionMode` to control context window size
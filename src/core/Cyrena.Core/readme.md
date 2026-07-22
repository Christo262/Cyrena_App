## Overview

`Cyrena.Core` is the foundational class library of the Cyréna AI assistant framework. It defines all core contracts, data models, application builders, and extension methods that the rest of the solution depends on. Any extension or plugin for Cyréna must reference this package.

**Root Namespace**: `Cyrena`  
**Project Type**: Class Library  
**Target Framework**: .NET 10.0

---

## Namespaces

| Namespace | Contents |
|-----------|----------|
| `Cyrena.Contracts` | All DI interfaces (`IAssistantMode`, `IAssistantPlugin`, `IChatMessageService`, etc.) |
| `Cyrena.Models` | Data models, entities, builders, event pipeline (`ChatConfiguration`, `CyrenaKernelBuilder`, `EventPipeline`, `JoinedKernelContent`, etc.) |
| `Cyrena.Options` | Application-level and chat-level options (`CyrenaBuilder`, `ChatOptions`) |
| `Cyrena.Extensions` | Extension methods for builders, services, and utilities |

---

## Contracts (`Cyrena.Contracts`)

### `IAssistantMode`
Configures behaviour surrounding the `Microsoft.SemanticKernel.Kernel` for a specific assistant mode.

```csharp
namespace Cyrena.Contracts
{
    public interface IAssistantMode
    {
        public const string AssistantModeDefault = "assistant-default";
        string Id { get; }
        Task ConfigureAsync(CyrenaKernelBuilder builder);
        Task DeleteAsync(ChatConfiguration config);
        Task EditAsync(ChatConfiguration config, IServiceProvider services);
    }
}
```

- `Id`: Unique identifier for the mode. Built-in default is `AssistantModeDefault = "assistant-default"`.
- `ConfigureAsync`: Called when a kernel is loaded for this mode. Use to add plugins, prompts, and services.
- `DeleteAsync`: Called when a chat using this mode is deleted.
- `EditAsync`: Called when a chat configuration is edited. Receives the scoped `IServiceProvider` of the current session.

### `IAssistantPlugin`
Adds additional services, features, or functions to an `IAssistantMode`.

```csharp
namespace Cyrena.Contracts
{
    public interface IAssistantPlugin
    {
        string Id { get; }
        string[] Modes { get; }      // Empty = applicable to all modes
        int Priority { get; }
        bool Required { get; }       // false = can be deactivated
        string Title { get; }
        Task LoadAsync(CyrenaKernelBuilder builder);
    }
}
```

### `IChatMessageService` (Kernel-locked, `IDisposable`)
Maintains chat history with separate kernel and display histories.

```csharp
namespace Cyrena.Contracts
{
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
}
```

- `KernelHistory` / `DisplayHistory`: filtered by `ChatOptions.IsKernelContent` / `IsDisplayContent`.
- `GetKernelHistory()`: returns the kernel history **after** `IConversationHistoryTransformer.TransformPreIterationHistory` has been applied.
- `Stream(string?)`: feed tokens as they arrive from the connection; subscribers added via `OnStreamToken` are invoked.
- `AddMessage`: the message's role determines whether it lands in kernel or display history (or both).

### `IChatConfigurationService`
Provides access to the current chat's persistent configuration.

```csharp
namespace Cyrena.Contracts
{
    public interface IChatConfigurationService
    {
        ChatConfiguration Config { get; }
        Task SaveConfigurationAsync();
    }
}
```

### `IConnection` (Kernel-locked)
Connection to an LLM service provider. Receives a `ChatMessageContent` to process.

```csharp
namespace Cyrena.Contracts
{
    public interface IConnection
    {
        Task HandleAsync(ChatMessageContent content, CancellationToken ct = default);
        void FunctionCallStart();
    }
}
```

- `HandleAsync`: Processes a chat message content (sends to LLM, handles response).
- `FunctionCallStart`: Called by function invocation filter to signal tool call start, helping suppress "thinking" messages.

### `IConnectionProvider`
Provides connections to LLM backends.

```csharp
namespace Cyrena.Contracts
{
    public interface IConnectionProvider
    {
        Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync();
        Task<bool> HasConnectionAsync(string id);
        Task<ConnectionInfo> AttachAsync(IKernelBuilder builder, string connectionId);
    }
}
```

### `IConversationHistoryTransformer` (Kernel-locked)
Used to modify the conversation history to ensure context stays short.

```csharp
namespace Cyrena.Contracts
{
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
}
```

### `ICyrenaFileExporter` (Kernel-locked)
Exports conversation files as a `.cyrena` zipped archive with manifest info.

```csharp
namespace Cyrena.Contracts
{
    public interface ICyrenaFileExporter
    {
        Task<CyrenaFileManifest> ExportFilesAsync(
            string extensionId,
            Version extensionVersion,
            string importerId,
            Dictionary<string, string?> properties,
            string outPath,
            CancellationToken cancellationToken = default);
    }
}
```

### `ICyrenaFileImporter` (Global service)
Handles processing of imported `.cyrena` files based on manifest. **Must be registered as a global singleton.**

```csharp
namespace Cyrena.Contracts
{
    public interface ICyrenaFileImporter
    {
        string Id { get; }
        Task ImportAsync(CyrenaFileManifest manifest, string absoluteDataPath, CancellationToken cancellationToken = default);
    }
}
```

### `IFileHandler` (Kernel-locked)
Handles file attachments in chat messages.

```csharp
namespace Cyrena.Contracts
{
    public interface IFileHandler
    {
        bool HandlesType(string contentType, string fileName);
        Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null);
        string[] GetSupportedMimeTypes();
        Dictionary<string, string> GetExtensionMimeTypeMapping();
    }
}
```

### `IFileHandlerFactory` (Kernel-locked)
Aggregates all `IFileHandler` instances in a kernel. Also manages file attachments persistence.

```csharp
namespace Cyrena.Contracts
{
    public interface IFileHandlerFactory
    {
        bool HasFileHandlers { get; }
        bool CanHandleType(string contentType, string fileName);
        string[] GetSupportedMimeTypes();
        Task<KernelContent> GetKernelContent(string fileId, CancellationToken cancellationToken = default);
        Task<KernelContent?> SaveAsync(Stream data, string contentType, string name, CancellationToken cancellationToken = default);
        Task<KernelContent?> SaveAsync(byte[] data, string contentType, string name, CancellationToken cancellationToken = default);
        Task CancelAsync(KernelContent item, CancellationToken cancellationToken = default);
        /// <summary>Returns the file extension (with leading '.') for a mime type, or null if unsupported.</summary>
        string? GetExtension(string mimeType);
        Task<IEnumerable<FileAttachment>> ListAttachmentsAsync(CancellationToken cancellationToken = default);
        Task<byte[]> GetFileDataAsync(string id, CancellationToken cancellationToken = default);
        Task DeleteFileAttachmentAsync(string id, CancellationToken cancellationToken = default);
        Task<FileAttachment> CreateAsync(string name, string contentType, byte[] content, CancellationToken cancellationToken = default);
        Task UpdateAsync(FileAttachment att, CancellationToken cancellationToken = default);
        Task<FileAttachment?> GetAttachmentAsync(string id, CancellationToken cancellationToken = default);
    }
}
```

Note: `GetKernelContent(string fileId, ...)` is non-nullable. It throws or returns an empty item rather than null when the id is unknown — implementations should be inspected to confirm exact behaviour.

### `IKernelController` (`IDisposable`)
Manages all `Kernel` instances. Creates, loads, updates, and unloads kernels when needed.

```csharp
namespace Cyrena.Contracts
{
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
}
```

`LoadAsync(ChatConfiguration)` throws `NullReferenceException` when the `IAssistantMode` is not found, and `InvalidOperationException` when the connection is not found.

### `IKernelResolver` (Kernel-locked)
Provides access to the current kernel instance via a factory function.

```csharp
namespace Cyrena.Contracts
{
    public interface IKernelResolver
    {
        Func<Kernel> Resolve { get; }
    }
}
```

### `IPromptManager` (Kernel-locked)
Dynamic system prompt configuration.

```csharp
namespace Cyrena.Contracts
{
    public interface IPromptManager
    {
        IReadOnlyList<Prompt> Prompts { get; }
        string AddPrompt(int order, string content);
        void UpdatePrompt(string id, string content);
        void RemovePrompt(string id);
    }
}
```

### `IIterationService` (Kernel-locked, `IDisposable`)
Manages a single chat iteration from user input to model completion.

```csharp
namespace Cyrena.Contracts
{
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
}
```

- `IterationId` is assigned when `InferenceStart` is called (a fresh `Ulid`).
- `OnIterationStart` / `OnIterationEnd` callback receives `true` when the iteration came from a queued input (not the primary user submission).
- `Iterate()` is the entry point: takes the current `Input` and enqueues/runs it. Implementation in `Cyrena.Runtime` polls a worker; see runtime review notes.

### `ISettingsService`
Encrypted settings storage. Generic over reference types.

```csharp
namespace Cyrena.Contracts
{
    public interface ISettingsService
    {
        void Save<T>(string key, T value) where T : class;
        T? Read<T>(string key) where T : class;
    }
}
```

### `IFileDialog`
Cross-platform file dialog abstraction.

```csharp
namespace Cyrena.Contracts
{
    public interface IFileDialog
    {
        Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? filter);
        Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? filter, string? defaultPath = null);
        void ExploreFolder(string folderPath);
        Task<string?> SelectFolder(string title = "Select Folder", string? current = null);
    }
}
```

### `IStartupTask`
Post-DI-build startup task with ordered execution.

```csharp
namespace Cyrena.Contracts
{
    public interface IStartupTask
    {
        int Order { get; }
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
```

### `IImportService`
Primary service for performing imports. Global (non-kernel-scoped) service.

```csharp
namespace Cyrena.Contracts
{
    public interface IImportService
    {
        bool HasImporters();
        Task StartImportAsync(CancellationToken cancellationToken = default);
    }
}
```

---

## Models (`Cyrena.Models`)

### `ChatConfiguration` (extends `Entity`)
Persistent configuration for each chat conversation.

```csharp
namespace Cyrena.Models
{
    public sealed class ChatConfiguration : Entity
    {
        public const string Icon = "icon";
        public const string Group = "group";

        // Constructor: generates Ulid Id, empty Properties and PluginIds.
        public ChatConfiguration();

        public string? this[string key] { get; set; }   // over Properties
        public string? Title { get; set; }
        public string AssistantModeId { get; set; } = default!;
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        [Required]
        public string ConnectionId { get; set; } = default!;
        public Dictionary<string, string?> Properties { get; set; }
        public List<string> PluginIds { get; set; }
        public string? WorkingDirectory { get; set; }   // backed by Properties["working.directory"]
        public string FileStoragePath { get; set; }     // backed by Properties["file_storage"]
        public HistoryInclusionMode HistoryInclusion { get; set; } = HistoryInclusionMode.All;
    }
}
```

`WorkingDirectory` and `FileStoragePath` are convenience accessors over `Properties`. Reading them returns null/empty when the underlying key is absent.

### `ChatMessageContentEntity` (extends `Entity`)
Used to persist chat history to storage.

```csharp
namespace Cyrena.Models
{
    public sealed class ChatMessageContentEntity : Entity
    {
        public ChatMessageContentEntity();
        public ChatMessageContentEntity(ChatMessageContent content, Ulid? iterationId = null);

        public Ulid? IterationId { get; set; }
        public DateTime Date { get; set; }
        public string? Role { get; set; }
        public List<KernelContentEntity> Items { get; set; }
        public string? ModelId { get; set; }                                              // [JsonIgnore(WhenWritingNull)]
        public IReadOnlyDictionary<string, object?>? Metadata { get; set; }              // [JsonIgnore(WhenWritingNull)]
        public string? MimeType { get; set; }                                            // [JsonIgnore(WhenWritingNull)]

        public ChatMessageContent AsChatMessage();
    }

    public sealed class KernelContentEntity : Entity
    {
        public KernelContentEntity();
        public KernelContentEntity(KernelContent item);

        public KernelContent? Item { get; set; }
    }
}
```

- `ChatMessageContentEntity(ChatMessageContent, Ulid?)` ctor unwraps `content.Items` into `KernelContentEntity` entries. Items where `Item == null` after construction are dropped.
- `KernelContentEntity(KernelContent item)` ctor: if `item.Metadata` contains a `"save-as"` key whose value is a `KernelContent`, that value becomes `Item` instead of the wrapper itself. This is how `JoinedKernelContent` unwraps to its inner content for persistence.

### `CyrenaFileManifest`
Represents the manifest inside a `.cyrena` zipped archive.

```csharp
namespace Cyrena.Models
{
    public sealed class CyrenaFileManifest
    {
        [JsonConstructor] internal CyrenaFileManifest();
        public CyrenaFileManifest(string extension, Version version, string importerId);

        [JsonPropertyName("extension.required")]
        public string Extension { get; set; } = default!;
        [JsonPropertyName("required.extension.version.min")]
        public Version Version { get; set; } = default!;
        [JsonPropertyName("importer.id")]
        public string ImporterId { get; set; } = default!;
        [JsonPropertyName("cyrena.properties")]
        public Dictionary<string, string?> Properties { get; set; }

        public string? this[string key] { get; set; }   // over Properties
    }
}
```

The `[JsonConstructor]` is `internal` — public consumers use the parameterized ctor.

### `CyrenaKernelBuilder`
Per-chat kernel configuration builder passed to modes and plugins.

```csharp
namespace Cyrena.Models
{
    public sealed class CyrenaKernelBuilder
    {
        public CyrenaKernelBuilder(ChatConfiguration chatConfiguration, IKernelBuilder kernelBuilder);

        public ChatConfiguration ChatConfiguration { get; }
        public IKernelBuilder KernelBuilder { get; }
        public IDictionary<string, object> FeatureOptions { get; }   // initialised empty
        public IServiceCollection Services => KernelBuilder.Services;
        public IKernelBuilderPlugins Plugins => KernelBuilder.Plugins;
    }
}
```

### `CyrenaBuilder` and `CyrenaOptions`
Application-level DI builder.

```csharp
namespace Cyrena.Options
{
    public sealed class CyrenaBuilder
    {
        // App-wide static paths
        public static readonly string AppDataDirectory      = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
        public static readonly string UserContentDirectory  = Path.Combine(AppDataDirectory, "public");
        public static readonly string ConversationsData     = Path.Combine(AppDataDirectory, "conversations");

        public CyrenaBuilder(IServiceCollection services);   // ctor required; services is held

        public IServiceCollection Services { get; }
        public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
        public IDictionary<string, object> FeatureOptions { get; }
        public IList<Action<CyrenaBuilder>> BuildActions { get; }
        public IList<Action<IServiceProvider, CancellationToken>> RunActions { get; }

        public void AddBuildAction(Action<CyrenaBuilder> action);
        public void AddRunAction(Action<IServiceProvider, CancellationToken> action);

        // Runs all BuildActions in order, then registers CyrenaOptions(FeatureAssemblies) as singleton.
        public void Build();
    }

    public sealed class CyrenaOptions
    {
        public CyrenaOptions(IDictionary<string, IList<Assembly>> featureAssemblies);
        public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
    }
}
```

### `EventPipeline`, `IEventPipe`, `EventPipe`, `EventPipe<T>`
Custom publish/subscribe event system with automatic cleanup.

```csharp
namespace Cyrena.Models
{
    public interface IEventPipe : IDisposable
    {
        void Invoke();
        void Invoke(object obj);
        bool IsDisposed { get; }
    }

    public class EventPipe : IEventPipe
    {
        public EventPipe(Action action);
        public void Dispose();                          // sets internal _disposed
        public void Invoke();
        public void Invoke(object obj);                 // calls parameterless Invoke (obj ignored)
        public bool IsDisposed { get; }
    }

    public class EventPipe<T> : IEventPipe
    {
        public EventPipe(Action<T> action);
        public void Dispose();
        public void Invoke();                           // throws NotImplementedException
        public void Invoke(object obj);                 // if obj is T, calls action
        public bool IsDisposed { get; }
    }

    public abstract class EventPipeline : IDisposable
    {
        protected EventPipeline();

        protected void InvokePipeline(string key);
        protected void InvokePipeline<T>(string key, T value);
        protected IDisposable ConfigurePipe(string key, Action cb);
        protected IDisposable ConfigurePipe<T>(string key, Action<T> cb);

        public void Dispose();                          // disposes all pipes across all keys
    }
}
```

- `InvokePipeline` snapshots the pipe list, invokes each, removes disposed/failed pipes, and swallows per-pipe exceptions (disposing the offending pipe).
- `EventPipe<T>.Invoke()` (parameterless) is intentionally a no-op throw — it exists only to satisfy `IEventPipe`. Use the typed `InvokePipeline<T>(key, value)` path.
- `EventPipe.Invoke(object obj)` ignores its argument and calls the parameterless overload.

### `Entity` / `IEntity`
Base entity with `Id`.

```csharp
namespace Cyrena.Models
{
    public interface IEntity { string Id { get; set; } }
    public abstract class Entity : IEntity
    {
        public virtual string Id { get; set; } = default!;
    }
}
```

### `ConnectionInfo`
LLM connection metadata record.

```csharp
namespace Cyrena.Models
{
    public record ConnectionInfo(
        string Id,
        string Name,
        string Source,         // e.g. "ollama", "openai"
        string ModelId,
        IConnectionProvider Provider,
        bool SupportImages,
        bool SupportFiles);
}
```

### `FileAttachment` (extends `Entity`)
Represents a persisted file attachment.

```csharp
namespace Cyrena.Models
{
    public sealed class FileAttachment : Entity
    {
        [JsonConstructor] internal FileAttachment();

        public string MimeType { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string InternalName { get; set; } = default!;
        public List<string> Tools { get; set; }     // initialised empty in ctor
        public Dictionary<string, string?> Properties { get; set; }
        public string? this[string key] { get; set; }

        public static FileAttachment From(string file_name, string content_type, string path, string original_name, params string[] tools);
        public FileReferenceContent ToFileReference();
    }
}
```

- `From(...)` defaults `tools` to `["Attachment_get"]` when the caller passes no tools.
- `ToFileReference()` returns a `FileReferenceContent` with `MimeType`, `Tools`, and a `Metadata["name"]` derived from the attachment's `Id`.

### `HistoryInclusionMode`
Controls how much chat history is sent to the AI.

```csharp
namespace Cyrena.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HistoryInclusionMode
    {
        All,        // Includes entire history
        LastTwo,    // Includes only last 2 iterations
        LastTen,    // Includes only last 10 iterations
        Instruct    // Includes no history, instruct mode
    }
}
```

### `JoinedKernelContent` (extends `KernelContent`)
Wraps multiple `KernelContent` items plus an optional "save-as" target, used to attach several pieces of content to one logical reference.

```csharp
namespace Cyrena.Models
{
    public sealed class JoinedKernelContent : KernelContent
    {
        public JoinedKernelContent(KernelContent[] contents, KernelContent? saveAs);
        public KernelContent[] Contents { get; }
        public KernelContent? SaveAs { get; }
    }
}
```

- The constructor copies `saveAs.Metadata` into the wrapper's `Metadata` under a `"save-as"` key pointing at the `saveAs` instance. This is what `KernelContentEntity(KernelContent)` looks for at persistence time to unwrap to the inner content.
- `Contents` is exposed for iteration; only `SaveAs` is what gets persisted.

### `Prompt`
System prompt with ordering.

```csharp
namespace Cyrena.Models
{
    public sealed class Prompt
    {
        public Prompt();                    // Id = Guid.NewGuid().ToString()
        public string Id { get; init; }
        public int Order { get; init; }
        public string Content { get; init; } = default!;
    }
}
```

All three properties are `init`-only; mutation requires re-creating the instance.

### `QueuedInput`
Represents a single queued chat input message.

```csharp
namespace Cyrena.Models
{
    public sealed class QueuedInput
    {
        public QueuedInput(ChatMessageContent message);
        public string Id { get; }                  // = Guid.NewGuid().ToString()
        public AuthorRole Role => Message.Role;
        public ChatMessageContent Message { get; set; }
    }
}
```

### `ToolResult` / `ToolResult<T>` and `ISuppressibleResult`
Function call result wrappers.

```csharp
namespace Cyrena.Models
{
    public interface ISuppressibleResult { string Suppress(); }

    public class ToolResult
    {
        public ToolResult();
        public ToolResult(bool success, string? message);
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ToolResult<T> : ToolResult, ISuppressibleResult
        where T : class
    {
        public ToolResult();
        public ToolResult(bool success, string? message) : base(success, message) { }
        public ToolResult(T result, bool success = true, string? message = null) : base(success, message) { Result = result; }
        public T? Result { get; set; }

        public string Suppress();   // delegates to Result.Suppress() if Result is ISuppressibleResult, else returns "[RESULT: {Success}, MESSAGE:{Message ?? "empty"}]"
    }
}
```

---

## Options (`Cyrena.Options`)

### `ChatOptions`
Role configuration for chat messages.

```csharp
namespace Cyrena.Options
{
    public sealed class ChatOptions
    {
        // Default ctor: sets System/Assistant/User/Tool to SK defaults, creates custom log roles,
        // sets MessagePersistRoles = [Assistant, User, Tool].
        public ChatOptions();

        // Full ctor: caller supplies every role; MessagePersistRoles still initialised to [Assistant, User, Tool].
        public ChatOptions(AuthorRole system, AuthorRole assistant, AuthorRole user, AuthorRole tool,
                          AuthorRole info, AuthorRole success, AuthorRole warn, AuthorRole error);

        public AuthorRole System { get; }
        public AuthorRole Assistant { get; }
        public AuthorRole User { get; }
        public AuthorRole Tool { get; }
        public AuthorRole LogInfo { get; }
        public AuthorRole LogSuccess { get; }
        public AuthorRole LogWarn { get; }
        public AuthorRole LogError { get; }
        public bool IncludeLogsInDisplay { get; set; } = true;
        public AuthorRole[] MessagePersistRoles { get; set; }
    }
}
```

The role properties are read-only — set them via ctor. `MessagePersistRoles` and `IncludeLogsInDisplay` are mutable.

---

## Extension Methods (`Cyrena.Extensions`)

### `CyrenaBuilderExtensions`

```csharp
namespace Cyrena.Extensions
{
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
}
```

`AddFeatureOption` throws `InvalidOperationException` if the type's name is already registered. `GetFeatureOption<T>` throws `NullReferenceException` when the key is absent.

### `CyrenaKernelBuilderExtensions`

```csharp
namespace Cyrena.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static void AddFeatureOption<T>(this CyrenaKernelBuilder builder, T option) where T : class;
        public static object? GetFeatureOption(this CyrenaKernelBuilder builder, string name);
        public static T GetFeatureOption<T>(this CyrenaKernelBuilder builder) where T : class;
    }
}
```

Same throw-on-duplicate / throw-on-missing semantics as the `CyrenaBuilder` overloads.

### `ChatMessageServiceExtensions`

```csharp
namespace Cyrena.Extensions
{
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
}
```

### `ChatOptionsExtensions`

```csharp
namespace Cyrena.Extensions
{
    public static class ChatOptionsExtensions
    {
        public static bool IsDisplayContent(this ChatOptions options, ChatMessageContent content);
        public static bool IsKernelContent(this ChatOptions options, ChatMessageContent content);
    }
}
```

`IsDisplayContent` returns true for User/Assistant/any Log role **and** when the message contains at least one `TextContent` item. `IsKernelContent` returns true for System/User/Assistant/Tool.

### `KernelBuilderExtensions`

```csharp
namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        public static void AddStartupTask<TStartupTask>(this IKernelBuilder builder) where TStartupTask : class, IStartupTask;
    }
}
```

Registers the task as a singleton against the kernel's `IServiceCollection`.

### `KernelContentExtensions`

```csharp
namespace Cyrena.Extensions
{
    public static class KernelContentExtensions
    {
#pragma warning disable SKEXP0110
        public static TextContent ToTextContent(this FileReferenceContent reference);
#pragma warning restore SKEXP0110
    }
}
```

Returns a `TextContent` formatted as `[Attachment: {FileId}, Content Type: {MimeType}, Tools: {tools joined with ", "}]` (empty tools list falls back to no suffix).

### `Resources`

```csharp
namespace Cyrena.Extensions
{
    public static class Resources
    {
        public static string Read(Assembly assembly, string resourceName);
    }
}
```

Reads a manifest resource from the assembly as UTF-8. Throws `FileNotFoundException(resourceName)` (single-arg ctor — note: the argument is the resource name, not a path) if the resource is not found.

---

## Usage for Extension Developers

Reference `Cyrena.Core` to:
1. Implement `IAssistantMode` or `IAssistantPlugin`
2. Access chat services like `IChatMessageService`, `IPromptManager`, `IIterationService`
3. Use `CyrenaBuilder` and `CyrenaKernelBuilder` to configure DI and kernel
4. Use `ChatConfiguration` and `Entity` for data models
5. Use `EventPipeline` (and the `IEventPipe` family) for custom events
6. Use `ToolResult` / `ToolResult<T>` for function call results
7. Use `IFileHandler` and `IFileHandlerFactory` for file attachment support
8. Use `IConversationHistoryTransformer` for history modification
9. Use `ISuppressibleResult` to reduce context size after tool calls
10. Use `ICyrenaFileExporter` and `ICyrenaFileImporter` for custom file import/export
11. Use `HistoryInclusionMode` to control context window size
12. Use `JoinedKernelContent` to attach multiple content items to a single reference
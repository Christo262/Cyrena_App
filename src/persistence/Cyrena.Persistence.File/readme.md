## Overview

`Cyrena.Persistence.File` is the JSON file-based persistence implementation for the Cyréna framework. It provides concrete implementations of the persistence contracts defined in `Cyrena.Persistence.Core`, storing entities as individual JSON files in a directory structure, with an in-memory cache per collection for performance.

**Version:** 0.6.0
**Target Framework:** .NET 10.0
**Namespaces:** `Cyrena.Contracts`, `Cyrena.Extensions`, `Cyrena.Options`, `Cyrena.Persistence.File.Options`, `Cyrena.Persistence.File.Services`, `Cyrena.Services`

---

## Contracts

### `IPersistenceFS` (`Cyrena.Contracts`)
File system abstraction for reading, writing, and deleting entity JSON files. Used by `FileStore<T>` for low-level file I/O.

```csharp
public interface IPersistenceFS
{
    IList<T> Read<T>(string collectionName) where T : class, IEntity;
    void Write<T>(T entity, string collectionName) where T : class, IEntity;
    void Delete<T>(string id, string collectionName) where T : class, IEntity;
}
```

**Methods:**
- `Read<T>(collectionName)` — Loads all entities from a collection directory, deserializing JSON files. Filters the in-memory cache by type `T`.
- `Write<T>(entity, collectionName)` — Writes an entity to a JSON file (creates or updates). Updates the in-memory cache first, then writes indented JSON to disk.
- `Delete<T>(id, collectionName)` — Removes an entity from the cache and deletes its JSON file by ID. No-op if the file does not exist.

---

## Services

### `FileStore<T>` (`Cyrena.Persistence.File.Services`, internal)
Implements `IStore<T>` using `IPersistenceFS` for JSON file-based storage. Each entity is stored as an individual JSON file in a collection directory.

```csharp
internal class FileStore<T> : IStore<T> where T : class, IEntity
```

**Constructors:**
- `FileStore(IPersistenceFS fs)` — Uses `typeof(T).Name` as the collection name.
- `FileStore(IPersistenceFS fs, string collectionName)` — Uses the supplied collection name.

**Behaviors:**
- `SaveAsync` / `AddAsync` auto-generate `Guid.NewGuid().ToString()` when `entity.Id` is null.
- `QueryableData` returns `_fs.Read<T>(_collectionName).AsQueryable()`.
- `FindManyAsync` applies `IOrderBy<T>` (ascending/descending) and `IPaging` (Skip/Take) when supplied.
- `DeleteManyAsync` evaluates the specification, deletes each match via `_fs.Delete`, and returns the count.
- `Dispose()` is a no-op.

**Thread safety:** Relies on `PersistenceFS` lock mechanism. `FileStore<T>` does not implement additional locking.

### `PersistenceFS` (`Cyrena.Services`, internal)
Concrete `IPersistenceFS` implementation with in-memory caching and thread-safe file I/O. Each collection is cached in memory after first read; writes synchronize both the cache and disk.

```csharp
internal class PersistenceFS : IPersistenceFS
```

**Constructors:**
- `PersistenceFS(IOptions<FilePersistenceOptions> options)` — From DI options.
- `PersistenceFS(FilePersistenceOptions options)` — From a direct options instance (used by `IsolatedFilePersistenceBuilder`).

**Fields:**
- `_options` (`FilePersistenceOptions`) — Configuration for `BaseDirectory` and `FileExtension`.
- `_collections` (`Dictionary<string, IList<IEntity>>`) — In-memory cache of loaded collections.
- `_lock` (`object`) — Lock used for all operations.

**Behaviors:**
- `Read<T>` — Acquires lock; returns cached entries filtered by `T` if the collection is loaded; otherwise loads via `FilePersistenceOptions.LoadList<T>`, caches the result as `List<IEntity>`, and returns the typed list.
- `Write<T>` — Acquires lock; ensures the collection is loaded; finds existing entity by `Id` in the cache and either appends or replaces it at the same index; serializes the entity with `JsonSerializerOptions { WriteIndented = true }` and writes to `{BaseDirectory}/{collectionName}/{entity.Id}.{FileExtension}`. Creates the collection directory if missing.
- `Delete<T>` — Acquires lock; ensures the collection is loaded; removes the matching entry from the cache; deletes the file if it exists.

**File layout:**
```
{BaseDirectory}/
  {collectionName}/
    {entityId}.{FileExtension}
```

---

## Options

### `FilePersistenceOptions` (`Cyrena.Options`)
Configuration for file-based persistence.

```csharp
public class FilePersistenceOptions
{
    public string BaseDirectory { get; set; } = "./data";
    public string FileExtension { get; set; } = "json";
}
```

| Property | Default | Description |
|----------|---------|-------------|
| `BaseDirectory` | `"./data"` | Root directory for all collection folders. |
| `FileExtension` | `"json"` | File extension for entity files (without dot). |

---

## Builders (`Cyrena.Persistence.File.Options`, both internal)

### `FilePersistenceBuilder`
Implements `ICyrenaPersistenceBuilder` for **standard (shared `PersistenceFS`)** file storage. Stores registered here resolve `IPersistenceFS` from DI.

```csharp
internal class FilePersistenceBuilder : ICyrenaPersistenceBuilder
{
    public FilePersistenceBuilder(IServiceCollection services);
    void ICyrenaPersistenceBuilder.AddScopedStore<TEntity>(string collectionName);
    void ICyrenaPersistenceBuilder.AddSingletonStore<TEntity>(string collectionName);
}
```

Both methods register `IStore<TEntity>` and resolve `IPersistenceFS` from DI inside the factory.

### `IsolatedFilePersistenceBuilder`
Implements `ICyrenaPersistenceBuilder` for **isolated (per-kernel) file storage**. Creates a dedicated `PersistenceFS` instance from the supplied `FilePersistenceOptions` and uses it directly — does not resolve `IPersistenceFS` from DI.

```csharp
internal class IsolatedFilePersistenceBuilder : ICyrenaPersistenceBuilder
{
    public IsolatedFilePersistenceBuilder(IServiceCollection services, FilePersistenceOptions options);
    void ICyrenaPersistenceBuilder.AddScopedStore<TEntity>(string collectionName);
    void ICyrenaPersistenceBuilder.AddSingletonStore<TEntity>(string collectionName);
}
```

Use this when data should not be shared across kernels (e.g. temporary chat data, session caches).

---

## Extension Methods (`Cyrena.Extensions`)

### `CyrenaBuilderExtensions.UseFilePersistence`
Configures file-based persistence at the **application level**.

```csharp
public static CyrenaBuilder UseFilePersistence(this CyrenaBuilder builder, Action<FilePersistenceOptions> options);
```

What it does:
1. Creates a `FilePersistenceBuilder` and registers it as `ICyrenaPersistenceBuilder` in `CyrenaBuilder.FeatureOptions`.
2. Registers `IPersistenceFS` as a singleton `PersistenceFS`.
3. Applies the supplied `FilePersistenceOptions` configuration.

```csharp
builder.UseFilePersistence(options =>
{
    options.BaseDirectory = "./my-data";
    options.FileExtension = "json";
});
```

### `KernelBuilderExtensions.AddFilePersistence`
Configures file-based persistence scoped to a specific kernel/chat.

```csharp
public static ICyrenaPersistenceBuilder AddFilePersistence(
    this CyrenaKernelBuilder builder, string path, string extension = "json");
```

What it does:
1. Configures `FilePersistenceOptions` with the provided `path` and `extension`.
2. Registers `IPersistenceFS` as a singleton `PersistenceFS`.
3. Creates a `FilePersistenceBuilder` and registers it as `ICyrenaPersistenceBuilder`.
4. Returns the `ICyrenaPersistenceBuilder` for further store registration.

```csharp
var persistence = builder.AddFilePersistence("./chat-data", "json");
persistence.AddScopedStore<ChatHistory>("history");
```

### `KernelBuilderExtensions.AddIsolatedFilePersistence`
Configures isolated file-based persistence for a kernel (data is not shared across kernels).

```csharp
public static void AddIsolatedFilePersistence(
    this CyrenaKernelBuilder builder, string path, Action<ICyrenaPersistenceBuilder> configure, string extension = "json");
```

What it does:
1. Creates `FilePersistenceOptions` from `path` and `extension`.
2. Creates an `IsolatedFilePersistenceBuilder` with a dedicated `PersistenceFS` instance.
3. Invokes `configure` to register stores.

```csharp
builder.AddIsolatedFilePersistence("./isolated-chat-data", persistence =>
{
    persistence.AddScopedStore<TemporaryData>("temp-data");
}, "json");
```

### `FilePersistenceExtensions`
Extension methods on `FilePersistenceOptions` for loading entity data from disk. Used internally by `PersistenceFS` to hydrate collections.

```csharp
public static class FilePersistenceExtensions
{
    public static IEnumerable<T> LoadEnumerable<T>(this FilePersistenceOptions options, string collectionName) where T : class, IEntity;
    public static IList<T> LoadList<T>(this FilePersistenceOptions options, string collectionName) where T : class, IEntity;
}
```

Both methods:
1. Construct `Path.Combine(options.BaseDirectory, collectionName)`.
2. Create the directory if it doesn't exist.
3. Read all `*.{options.FileExtension}` files in the directory.
4. Deserialize each file as JSON to type `T`.
5. Skip null results and return the collection.

`LoadList` returns a `List<T>` (eager); `LoadEnumerable` returns the same items via `IEnumerable<T>` (currently also eager, but typed as a sequence for future lazy loading).

---

## Usage for Extension Developers

Reference `Cyrena.Persistence.File` to:
1. Add file-based persistence to your application or kernel.
2. Store entities as JSON files in a directory structure.
3. Use in-memory caching with automatic disk synchronization.

**Example - Application-level persistence:**
```csharp
// In Program.cs or app startup
builder.UseFilePersistence(options =>
{
    options.BaseDirectory = Path.Combine(appDataPath, "CyrenaData");
    options.FileExtension = "json";
});

// Then register stores
builder.AddScopedStore<MyEntity>("my-entities");
```

**Example - Kernel-scoped persistence:**
```csharp
// In IAssistantPlugin.LoadAsync
var persistence = builder.AddFilePersistence("./chat-data", "json");
persistence.AddScopedStore<ChatHistory>("history");
```

**Example - Isolated persistence:**
```csharp
// In IAssistantPlugin.LoadAsync
builder.AddIsolatedFilePersistence("./isolated-chat-data", persistence =>
{
    persistence.AddScopedStore<TemporaryData>("temp-data");
}, "json");
```

### Related
- `IStore<T>` — Generic repository contract
- `ICyrenaPersistenceBuilder` — Builder interface
- `Cyrena.Persistence.Core` — Persistence abstraction layer
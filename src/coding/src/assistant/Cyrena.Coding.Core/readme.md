## Overview

`Cyrena.Coding.Core` is the foundational class library for the Cyréna coding assistant. It defines all contracts, models, extensions, and configuration constants that extensions and plugins use to build project-aware AI coding capabilities.

**Package:** `Cyrena.Coding.Core`  
**Namespace:** `Cyrena.Coding` (contracts, models, options, extensions)  
**Target Framework:** `net10.0`  
**Dependency:** `Cyrena.Core` (provides `Entity`, `ChatConfiguration`, `CyrenaKernelBuilder`)

---

## Contracts

### `ICodeBuilder`

**Namespace:** `Cyrena.Coding.Contracts`

Defines how different project types configure the AI assistant's `DevelopPlan`, register Semantic Kernel plugins, and set up prompts. Implement this to add support for a new project type (e.g., Rust, Go, custom framework).

```csharp
public interface ICodeBuilder
{
    /// <summary>Project type identifier stored in ChatConfiguration</summary>
    string Id { get; }

    /// <summary>Configures plugins/services and creates the DevelopPlan</summary>
    Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options);

    Task DeleteAsync(ChatConfiguration config);
    Task EditAsync(ChatConfiguration config, IServiceProvider services);
}
```

**Registration pattern:**
```csharp
builder.Services.AddSingleton<ICodeBuilder, MyProjectBuilder>();
```

**Selection at runtime:** `config[DevelopOptions.BuilderId]` must match `ICodeBuilder.Id`.

**Typical `ConfigureAsync` implementation:**
1. Create `DevelopPlan` from `options.ChatConfiguration.WorkingDirectory`
2. Index files using `plan.IndexFiles()` or folder-specific extensions
3. Register Semantic Kernel plugins via `options.Plugins.AddFromType<T>()`
4. Add system prompts via `options.GetFeatureOption<IPromptManager>().AddPrompt()`
5. Register additional services in `options.Services`
6. Return the `DevelopPlan`

---

### `IDevelopPlanService`

**Namespace:** `Cyrena.Coding.Contracts`

Provides access to the current `DevelopPlan` and observable hooks for plan and file lifecycle events. Used for project switching in multi-project solutions.

```csharp
public interface IDevelopPlanService
{
    DevelopPlan Plan { get; }
    void SetPlan(DevelopPlan newPlan);

    IDisposable OnDevelopPlanChanged(Action<DevelopPlan> plan);
    IDisposable OnFileCreated(Action<DevelopFile> cb);
    IDisposable OnFileUpdated(Action<DevelopFile> cb);
    IDisposable OnFileDeleted(Action<DevelopFile> cb);

    void InvokeFileCreated(DevelopFile file);
    void InvokeFileUpdated(DevelopFile file);
    void InvokeFileDeleted(DevelopFile file);
}
```

| Member | Description |
|--------|-------------|
| `Plan` | Current `DevelopPlan` instance |
| `SetPlan(DevelopPlan)` | Replaces the plan (project switching) |
| `OnDevelopPlanChanged` | Subscribe to plan replacement events |
| `OnFileCreated` / `OnFileUpdated` / `OnFileDeleted` | Subscribe to file lifecycle events |
| `InvokeFileCreated` / `InvokeFileUpdated` / `InvokeFileDeleted` | Raise events from plugins/services |

**Registration:** Singleton, typically instantiated with initial plan in `IAssistantMode.ConfigureAsync`.

---

### `IDevelopPlanIndexer`

**Namespace:** `Cyrena.Coding.Contracts`

Allows the `DevelopPlan` to be refreshed in `IDevelopPlanService` when `IIterationService.OnIterationStart` is triggered. Kernel-locked.

```csharp
public interface IDevelopPlanIndexer
{
    /// <summary>
    /// Refreshes the current plan. Returns a new <see cref="DevelopPlan"/> or null if no refresh is needed.
    /// </summary>
    DevelopPlan? RefreshPlan(DevelopPlan current);
}
```

**Usage:** Implementations are registered as singletons and called at the start of each AI iteration to re-index files that may have changed on disk.

---

### `IVersionControl`

**Namespace:** `Cyrena.Coding.Contracts`

Capped, timestamped in-memory version history for files modified by AI. Thread-safe. Each file tracks up to `MaxVersionsPerFile` snapshots (oldest dropped first).

```csharp
public interface IVersionControl
{
    int MaxVersionsPerFile { get; set; }

    // Write
    void Backup(DevelopFileContent? file, string? label = null);
    void RemoveBackup(string fileId);
    void Clear();

    // Query
    bool HasBackup(string fileId);
    DevelopFileVersion? GetLatest(string fileId);
    IReadOnlyList<DevelopFileVersion> GetHistory(string fileId);
    IEnumerable<DevelopFileVersion> GetAllLatest();

    // Restore by index or timestamp
    bool TryGetVersion(string fileId, int index, out DevelopFileVersion? version);
    bool TryGetVersionAt(string fileId, DateTimeOffset at, out DevelopFileVersion? version);

    // Rollback
    DevelopFileVersion? RollbackTo(DevelopFileVersion version);
    DevelopFileVersion? RollbackOne(string fileId);

    // Backward-compatible shims
    DevelopFileContent? GetBackups(string fileId);
    IEnumerable<DevelopFileContent> GetBackups();
}
```

**Default `MaxVersionsPerFile`:** 20  
**Usage pattern:**
```csharp
// Before any file modification:
_plan.Plan.TryReadFileContent(file, out var fileContent);
_version.Backup(fileContent);
// ... perform modification ...
```

---

## Models

### `DevelopPlan`

**Namespace:** `Cyrena.Coding.Models`

Root model representing the in-memory index of a project's files and folders. Implements `ISuppressibleResult`.

```csharp
public class DevelopPlan : ISuppressibleResult
{
    public DevelopPlan(string rootDirectory);
    
    [JsonConstructor]
    internal DevelopPlan();  // Internal, used only by JSON deserialization

    [JsonIgnore] public string RootDirectory { get; set; }
    [JsonIgnore] public string DataDirectory { get; set; }  // RootDirectory + "/.cyrena"

    public List<DevelopFile> Files { get; set; }
    public List<DevelopFolder> Folders { get; set; }

    public string Suppress() => "[PLAN:omitted; use Project_get_plan]";
}
```

**Constructor behavior:**
- Public constructor: Sets `RootDirectory`, `DataDirectory = Path.Combine(rootDirectory, ".cyrena")`, initializes empty `Files` and `Folders` lists.
- Internal `[JsonConstructor]` constructor: Initializes empty collections, sets `RootDirectory` and `DataDirectory` to `string.Empty`.

**`ISuppressibleResult`:** The `Suppress()` method returns a compact string representation for AI consumption, directing the AI to use `Project_get_plan` for full plan access.

---

### `DevelopItem` (abstract base)

**Namespace:** `Cyrena.Coding.Models`

Abstract base for all plan items. Extends `Cyrena.Models.Entity`.

```csharp
public abstract class DevelopItem : Entity
{
    public string Name { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
}
```

---

### `DevelopFile`

**Namespace:** `Cyrena.Coding.Models`

Represents a file in the plan index. Implements `ISuppressibleResult`.

```csharp
public class DevelopFile : DevelopItem, ISuppressibleResult
{
    public bool ReadOnly { get; set; } = false;

    public string Suppress()
    {
        return ReadOnly
            ? $"[FILE:{Id}; read-only; content omitted; use Code_read/Code_read_lines]"
            : $"[FILE:{Id}; content omitted; use Code_read/Code_read_lines before editing]";
    }
}
```

**ReadOnly:** When `true`, the AI should not modify this file. Used for build config, project files, etc.

**`ISuppressibleResult`:** The `Suppress()` method returns a compact string for AI consumption. For read-only files, it includes a `read-only` marker.

---

### `DevelopFileContent`

**Namespace:** `Cyrena.Coding.Models`

File model with full text content.

```csharp
public class DevelopFileContent : DevelopFile
{
    public DevelopFileContent() { }
    public DevelopFileContent(DevelopFile file, string? content);
    public string? Content { get; set; }
}
```

The copy constructor copies `Id`, `Name`, `RelativePath`, `ReadOnly` from the source `DevelopFile` and sets `Content`.

---

### `DevelopFileLines`

**Namespace:** `Cyrena.Coding.Models`

File model with line-indexed content for precise line-based editing.

```csharp
public class DevelopFileLines : DevelopFile
{
    public DevelopFileLines();
    public DevelopFileLines(DevelopFile file, string? content);
    public List<DevelopFileLine> Lines { get; set; }
    public int LineCount => Lines.Count;
    public override string ToString() => string.Join("\n", Lines.OrderBy(x => x.Index).Select(x => x.Text ?? string.Empty));
}
```

**Constructor behavior:**
- Parameterless constructor initializes `Lines` to an empty `List<DevelopFileLine>`.
- Copy constructor copies `Id`, `Name`, `RelativePath`, `ReadOnly` from the source `DevelopFile`, then parses `content` into `Lines`.

**Line splitting behavior:**
- Normalizes all line endings to `\n` (replaces `\r\n` → `\n`, then `\r` → `\n`)
- Splits on `\n` with `StringSplitOptions.None` (preserves empty lines)
- Reconstructs via `ToString()` using `\n` only

---

### `DevelopFileLine`

**Namespace:** `Cyrena.Coding.Models`

Represents a single line within a `DevelopFileLines` model.

```csharp
public class DevelopFileLine
{
    public int Index { get; set; }
    public string? Text { get; set; }
}
```

| Property | Description |
|----------|-------------|
| `Index` | 0-based line index |
| `Text` | The line content (may be null or empty) |

---

### `DevelopFolder`

**Namespace:** `Cyrena.Coding.Models`

Represents a folder in the plan index. Supports nested folders and files.

```csharp
public class DevelopFolder : DevelopItem
{
    public DevelopFolder();
    public List<DevelopFile> Files { get; set; }
    public List<DevelopFolder> Folders { get; set; }
}
```

**Constructor:** Initializes `Files` and `Folders` to empty `List<T>` instances.

---

### `DevelopFileVersion`

**Namespace:** `Cyrena.Coding.Models`

Represents a single versioned snapshot of a file's content.

```csharp
public class DevelopFileVersion
{
    public DevelopFileVersion(DevelopFileContent file, string? label = null);
    public DevelopFileContent File { get; }
    public DateTimeOffset Timestamp { get; }
    public string? Label { get; }
    public override string ToString() => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {File.Name}{(Label != null ? $" — {Label}" : string.Empty)}";
}
```

**Constructor:** Sets `Timestamp` to `DateTimeOffset.UtcNow` at creation time.

---

### `ConsoleOutput`

**Namespace:** `Cyrena.Coding.Models`

Captures standard output from a process and returns it to the AI. Implements `ISuppressibleResult`. Thread-safe via internal locking.

```csharp
public sealed class ConsoleOutput : List<ConsoleLine>, ISuppressibleResult
{
    public ConsoleOutput();
    public string? Command { get; set; }
    public void WriteLine(string level, string? content);
    public string Suppress();
}
```

| Member | Description |
|--------|-------------|
| `Command` | The command that produced this output |
| `WriteLine` | Thread-safe append of a `ConsoleLine` |
| `Suppress()` | Returns compact summary: `"{command}: {level}_items={count}, ..."` |

**Thread safety:** `WriteLine` uses an internal `lock` object.

---

### `ConsoleLine`

**Namespace:** `Cyrena.Coding.Models`

Represents a single line of console output.

```csharp
public class ConsoleLine
{
    public string Level { get; set; } = "info";
    public string? Content { get; set; }
}
```

| Property | Description |
|----------|-------------|
| `Level` | Log level (default `"info"`) |
| `Content` | The line content |

---

### `StickyNote`

**Namespace:** `Cyrena.Coding.Models`

Simple entity for the AI to persist notes about the project across sessions.

```csharp
public sealed class StickyNote : Entity
{
    public StickyNote() { }
    public StickyNote(string? title, string? content);
    public string? Title { get; set; }
    public string? Content { get; set; }
}
```

**Constructor behavior:** The parameterized constructor generates a new `Guid.NewGuid().ToString()` for `Id`.

**Storage:** Typically stored via `IStore<StickyNote>` (from Cyrena.Persistence).  
**AI Access:** Exposed via `ProjectInformation` Semantic Kernel plugin functions (`get_plan`, `create_note`, `update_note`, `delete_note`).

---

## Extension Methods

### `DevelopFileExtensions`

**Namespace:** `Cyrena.Coding.Extensions`
**Target:** `DevelopPlan`

All methods operate on the plan's `RootDirectory` for disk I/O.

| Method | Signature | Purpose |
|--------|-----------|---------|
| `CreateFile` | `(plan, fileId, fileName, content)` → `DevelopFile` | Create file in root. If `fileId` exists, ensures file on disk and returns existing model. |
| `CreateFile` | `(plan, folder, fileId, fileName, content)` → `DevelopFile` | Create file in folder. Same idempotency behavior. |
| `TryReadFileContent` | `(plan, file, out content)` → `bool` | Read full content. Returns `false` with `null` **without mutating plan** if file missing on disk. |
| `TryReadFileLines` | `(plan, file, out lines)` → `bool` | Read as line-indexed model. Same non-mutating failure behavior. |
| `TryWriteFileContent` | `(plan, file, content, out fileContent)` → `bool` | Overwrite file content on disk. |
| `TryWriteFileOverwrite` | `(plan, file, content, out lines)` → `bool` | Overwrite entire file and return `DevelopFileLines`. |
| `TryWriteFileReplace` | `(plan, file, content, startLine, lineCount, out lines, out totalLines)` → `bool` | Replace a range of lines (0-based `startLine`, `lineCount` lines). Returns `false` if bounds invalid. |
| `TryWriteFileInsert` | `(plan, file, content, startLine, out lines, out totalLines)` → `bool` | Insert content before specified line (0-based). Use `startLine == totalLines` to append. |
| `RemoveFile` | `(plan, file)` → `bool` | Delete file from disk and remove from plan (root or nested). |
| `TryFindFile` | `(plan, fileId, out file, recursive)` → `bool` | Find by ID. Recursive by default. |
| `TryFindFile` | `(plan, folder, fileId, out file, recursive)` → `bool` | Find by ID within folder subtree. |
| `TryFindFileByName` | `(plan, name, out file, recursive)` → `bool` | Find by name (case-insensitive). |
| `TryFindFileByName` | `(plan, folder, name, out file, recursive)` → `bool` | Find by name within folder subtree. |
| `IndexFiles` | `(plan, extension, id_prefix, readOnly)` | Auto-index root files by extension. |
| `IndexFiles` | `(plan, folder, extension, id_prefix, readOnly)` | Auto-index folder files. |

**Important behavioral notes:**
- `TryReadFileContent` / `TryReadFileLines` are pure query methods. If the file does not exist on disk, they return `false` with `null` output **without** modifying the plan. Callers must explicitly call `RemoveFile` if they want to purge stale entries.
- `CreateFile` is idempotent by `fileId`. If a file with the given ID already exists in the plan, it ensures the file exists on disk (writing content if missing) and returns the existing model. It does not create duplicates.
- `IndexFiles` extension stripping uses suffix-only removal (`EndsWith` + range indexer), so files like `my.component.ts` correctly produce `my.component` rather than `my.componen`.
- `TryWriteFileReplace` validates that `startLine >= 0`, `lineCount > 0`, and `startLine + lineCount <= totalLines`. Returns `false` if any bound is invalid.
- `TryWriteFileInsert` allows `startLine == totalLines` to append after the last line. Returns `false` if `startLine < 0` or `startLine > totalLines`.
- All write methods normalize line endings to `\n` before writing to disk.

---

### `DevelopFolderExtensions`

**Namespace:** `Cyrena.Coding.Extensions`
**Target:** `DevelopPlan` and `DevelopFolder`

| Method | Signature | Purpose |
|--------|-----------|---------|
| `CreateFolder` | `(plan, id, name)` → `DevelopFolder` | Create folder in root. Idempotent by ID. |
| `CreateFolder` | `(plan, parent, id, name)` → `DevelopFolder` | Create nested folder. Uses `parent.RelativePath` for disk path. |
| `RemoveFolder` | `(plan, folder, recursive)` → `bool` | Delete folder from disk and plan. |
| `TryFindFolder` | `(plan, folderId, out folder, recursive)` → `bool` | Find by ID. |
| `TryFindFolder` | `(folder, folderId, out model, recursive)` → `bool` | Find by ID within folder subtree. |
| `GetFolderOfFile` | `(plan, file)` → `DevelopFolder?` | Find containing folder (root-level search). |
| `GetFolderOfFile` | `(plan, folder, file)` → `DevelopFolder?` | Find containing folder within subtree. |
| `GetOrCreateFolder` | `(plan, id, name)` → `DevelopFolder` | Get existing or create in root. |
| `GetOrCreateFolder` | `(plan, parent, id, name)` → `DevelopFolder` | Get existing or create nested. Searches within `parent` only (non-recursive). |

---

## Options

### `DevelopOptions`

**Namespace:** `Cyrena.Coding.Options`

Constant string keys used in `ChatConfiguration` dictionary for developer mode setup.

```csharp
public sealed class DevelopOptions
{
    public const string AssistantModeId = "developer";
    public const string BuilderId = "dev.builder-id";
}
```

| Constant | Value | Purpose |
|----------|-------|---------|
| `AssistantModeId` | `"developer"` | ID for the developer assistant mode |
| `BuilderId` | `"dev.builder-id"` | Key storing the selected `ICodeBuilder.Id` |

**Removed:** `RootDirectory` (`"dev.root-dir"`) was previously defined but is now commented out with `[Obsolete]`. Extensions should use `ChatConfiguration.WorkingDirectory` instead.

**Usage:**
```csharp
// Setting configuration
model[DevelopOptions.BuilderId] = "cs-class-library";

// Reading configuration
var builderId = config[DevelopOptions.BuilderId];
```

---

## Dependencies on Cyrena.Core

This package depends on `Cyrena.Core` and uses the following types extensively. Extension developers must reference `Cyrena.Core` alongside this package.

| Type | Source | Usage |
|------|--------|-------|
| `Entity` | `Cyrena.Models` | Base for `DevelopItem`, `StickyNote` |
| `ChatConfiguration` | `Cyrena.Models` | Passed to `ICodeBuilder.DeleteAsync` / `EditAsync` |
| `CyrenaKernelBuilder` | `Cyrena` | Passed to `ICodeBuilder.ConfigureAsync` |
| `ISuppressibleResult` | `Cyrena` | Implemented by `DevelopFile`, `DevelopPlan`, `ConsoleOutput` |

---

## Class Hierarchy

```
Cyrena.Models.Entity
├── DevelopItem (abstract)
│   ├── DevelopFile
│   │   ├── DevelopFileContent
│   │   └── DevelopFileLines
│   └── DevelopFolder
├── StickyNote

DevelopPlan (ISuppressibleResult)
DevelopFileVersion
DevelopFileLine
ConsoleOutput (ISuppressibleResult) → List<ConsoleLine>
ConsoleLine
```

---

## Extension Developer Quick Start

1. **Reference packages:**
   ```xml
   <PackageReference Include="Cyrena.Core" />
   <PackageReference Include="Cyrena.Coding.Core" />
   ```

2. **Implement `ICodeBuilder`:**
   ```csharp
   public class MyProjectBuilder : ICodeBuilder
   {
       public string Id => "my-project";
       
       public async Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
       {
           var rootDir = options.ChatConfiguration.WorkingDirectory;
           var plan = new DevelopPlan(rootDir);
           // Index files, register plugins, add prompts...
           return plan;
       }
       
       public Task DeleteAsync(ChatConfiguration config) => Task.CompletedTask;
       public Task EditAsync(ChatConfiguration config, IServiceProvider services) => Task.CompletedTask;
   }
   ```

3. **Register in your extension:**
   ```csharp
   builder.Services.AddSingleton<ICodeBuilder, MyProjectBuilder>();
   ```

4. **Use plan extensions for file operations:**
   ```csharp
   plan.CreateFile("readme", "README.md", "# My Project");
   plan.CreateFolder("src", "src");
   plan.IndexFiles("cs", "src_");
   ```
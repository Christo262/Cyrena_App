# Cyrena.Coding.Core SDK

## Overview

**Namespace:** `Cyrena.Coding`  
**Project:** `Cyrena.Coding.Core.csproj`  
**Target Framework:** `.NET 10.0`  
**Dependencies:** `Cyrena.Core`, `Microsoft.Extensions.DependencyInjection.Abstractions`

This SDK provides developer workflow abstractions for AI agents to manage code projects, perform file system operations, and persist development state.

---

## Models

### DevelopItem (Abstract Base Class)

```csharp
public abstract class DevelopItem : Entity
```

**Namespace:** `Cyrena.Coding.Models`

Base class for `DevelopFile` and `DevelopFolder`. Extends `Entity` and implements `IJsonSerializable`.

**Properties:**
- `string Id` - Unique identifier (inherited from `Entity`)
- `string Name` - Display name
- `string RelativePath` - Path relative to project root

**JSON Serialization:** Implements `ToJson()` and `ToString()` methods using `JsonConvert.SerializeObject`.

---

### DevelopFile

```csharp
public class DevelopFile : DevelopItem
```

**Namespace:** `Cyrena.Coding.Models`

Represents a single file in the project tree.

**Properties:**
- `bool ReadOnly` - Indicates if file should not be modified (default: `false`)

**Inherited from DevelopItem:**
- `string Id`
- `string Name`
- `string RelativePath`

---

### DevelopFileContent

```csharp
public class DevelopFileContent : DevelopFile
```

**Namespace:** `Cyrena.Coding.Models`

Extends `DevelopFile` with editable content for file content operations.

**Constructors:**
```csharp
public DevelopFileContent()
public DevelopFileContent(DevelopFile file, string? content)
```

**Properties:**
- `string? Content` - Full file content as string

**Inherited from DevelopFile:**
- `string Id`
- `string Name`
- `string RelativePath`
- `bool ReadOnly`

**Usage:** Created by `DevelopFileExtensions.TryReadFileContent()`.

---

### DevelopFileLines

```csharp
public class DevelopFileLines : DevelopFile
```

**Namespace:** `Cyrena.Coding.Models`

Represents a file as a dictionary of line numbers to line content. Used for line-level operations.

**Constructor:**
```csharp
public DevelopFileLines(DevelopFile file, string? content)
```

**Properties:**
- `Dictionary<int, string> Lines` - Line number (0-based) to content mapping

**Line Ending Handling:**
- Detects `\r\n` (Windows) and `\n` (Unix) line endings
- Preserves empty lines using `StringSplitOptions.None`
- `ToString()` reconstructs content with Windows-style line endings (`\r\n`)

**Usage:** Created by `DevelopFileExtensions.TryReadFileLines()` and `TryWriteFileLine()`.

---

### DevelopFolder

```csharp
public class DevelopFolder : DevelopItem
```

**Namespace:** `Cyrena.Coding.Models`

Represents a folder containing files and nested folders.

**Properties:**
- `List<DevelopFile> Files` - Direct file children
- `List<DevelopFolder> Folders` - Nested folder children

**Inherited from DevelopItem:**
- `string Id`
- `string Name`
- `string RelativePath`

---

### DevelopPlan

```csharp
public class DevelopPlan 
```

**Namespace:** `Cyrena.Coding.Models`

Root container representing an entire code project plan. This is the central object for project operations.

**Constructors:**
```csharp
public DevelopPlan(string rootDirectory)
internal DevelopPlan()  // JSON deserialization constructor
```

**Properties:**
- `string RootDirectory` - Absolute path to project root (`[JsonIgnore]`)
- `string DataDirectory` - Path to `.cyrena` data folder (`[JsonIgnore]`)
- `List<DevelopFile> Files` - Root-level files
- `List<DevelopFolder> Folders` - Root-level folders

**Static Methods:**
```csharp
public static bool TryLoadFromDirectory(string dir, out DevelopPlan plan)
```
Attempts to load plan from `.cyrena/plan` file. Returns `true` if successful.

```csharp
public static void Save(DevelopPlan plan)
```
Saves plan to `.cyrena/plan` file.

**Instance Methods:**
```csharp
public string ToJson()
public override string ToString()
```
Returns JSON representation of the plan.

---

### StickyNote

```csharp
public sealed class StickyNote : Entity
```

**Namespace:** `Cyrena.Coding.Models`

Represents a persistent AI note with title and content.

**Constructors:**
```csharp
public StickyNote()
public StickyNote(string? title, string? content)
```

**Properties:**
- `string Id` - Unique identifier (auto-generated GUID)
- `string? Title` - Note title
- `string? Content` - Note body content

---

## Contracts (Interfaces)

### ICodeBuilder

```csharp
public interface ICodeBuilder
```

**Namespace:** `Cyrena.Coding.Contracts`

Contract for creating and configuring the development kernel and project plans.

**Properties:**
```csharp
string Id { get; }
```
Sets `ChatConfiguration` project type identifier.

**Methods:**
```csharp
Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
```
Configures additional plugins/services and creates the `DevelopPlan`.

```csharp
Task DeleteAsync(ChatConfiguration config)
```

```csharp
Task EditAsync(ChatConfiguration config, IServiceProvider services)
```

---

### IDevelopPlanService

```csharp
public interface IDevelopPlanService
```

**Namespace:** `Cyrena.Coding.Contracts`

Contract for accessing the current `DevelopPlan`. Allows changing the plan when switching between referenced projects.

**Properties:**
```csharp
DevelopPlan Plan { get; }
```

**Methods:**
```csharp
void SetPlan(DevelopPlan newPlan)
```

---

### IVersionControl

```csharp
public interface IVersionControl
```

**Namespace:** `Cyrena.Coding.Contracts`

Used to keep in-memory backup of files modified by AI.

**Methods:**
```csharp
void Backup(DevelopFileContent? file)
```
Stores a backup of the file content.

```csharp
bool HasBackup(string fileId)
```
Returns `true` if a backup exists for the given file ID.

```csharp
DevelopFileContent? GetBackups(string fileId)
```
Returns all backups for a file, or `null` if none exist.

```csharp
IEnumerable<DevelopFileContent> GetBackups()
```
Returns all stored backups.

```csharp
void RemoveBackup(string fileId)
```
Removes all backups for a file.

---

## Extensions

### DevelopFileExtensions

```csharp
public static class DevelopFileExtensions
```

**Namespace:** `Cyrena.Coding.Extensions`

**All methods extend `DevelopPlan`, not `DevelopFile` or `DevelopFolder`.**

**File Creation:**
```csharp
public static DevelopFile CreateFile(this DevelopPlan plan, string fileId, string fileName, string? content)

public static DevelopFile CreateFile(this DevelopPlan plan, DevelopFolder folder, string fileId, string fileName, string? content)
```
Creates a file in the root directory or within a folder. Returns existing file if `fileId` already exists. Writes content to disk using `File.WriteAllText`.

**Content Reading:**
```csharp
public static bool TryReadFileContent(this DevelopPlan plan, DevelopFile file, out DevelopFileContent? content)

public static bool TryReadFileLines(this DevelopPlan plan, DevelopFile file, out DevelopFileLines? lines)
```
Reads file content as string or lines dictionary. Returns `false` and removes file from plan if file doesn't exist on disk.

**Content Writing:**
```csharp
public static bool TryWriteFileContent(this DevelopPlan plan, DevelopFile file, string? content, out DevelopFileContent? fileContent)

public static bool TryWriteFileLine(this DevelopPlan plan, DevelopFile file, int index, string line, out DevelopFileLines? lines)
```
Writes full content or a single line. `TryWriteFileLine` validates index is within bounds (0 to `Lines.Count - 1`). Returns `false` if file doesn't exist.

**File Removal:**
```csharp
public static bool RemoveFile(this DevelopPlan plan, DevelopFile file)

public static bool RemoveFile(this DevelopPlan pl, DevelopFolder folder, DevelopFile file)
```
Removes file from disk and from the plan. Recursive search through folders. Returns `true` if removed.

**Search:**
```csharp
public static bool TryFindFile(this DevelopPlan plan, string fileId, out DevelopFile? file, bool recursive = true)

public static bool TryFindFile(this DevelopPlan pl, DevelopFolder folder, string fileId, out DevelopFile? file, bool recursive = true)

public static bool TryFindFileByName(this DevelopPlan plan, string name, out DevelopFile? file, bool recursive = true)

public static bool TryFindFileByName(this DevelopPlan plan, DevelopFolder folder, string name, out DevelopFile? file, bool recursive = true)
```
Search by ID or name. Case-insensitive name comparison. Non-recursive by default searches root-level only.

**Indexing:**
```csharp
public static void IndexFiles(this DevelopPlan plan, DevelopFolder folder, string extension, string id_prefix, bool readOnly = false)

public static void IndexFiles(this DevelopPlan plan, string extension, string id_prefix, bool readOnly = false)
```
Scans directory for files with matching extension and adds them to the plan. Also removes entries for files that no longer exist on disk.

---

### DevelopFolderExtensions

```csharp
public static class DevelopFolderExtensions
```

**Namespace:** `Cyrena.Coding.Extensions`

**Methods extend `DevelopPlan`.**

**Folder Creation:**
```csharp
public static DevelopFolder CreateFolder(this DevelopPlan plan, string id, string name)

public static DevelopFolder CreateFolder(this DevelopPlan plan, DevelopFolder folder, string id, string name)
```
Creates folder on disk and adds to plan. Returns existing folder if `id` already exists.

```csharp
public static DevelopFolder GetOrCreateFolder(this DevelopPlan plan, string id, string name)

public static DevelopFolder GetOrCreateFolder(this DevelopPlan plan, DevelopFolder parent, string id, string name)
```
Tries to find existing folder first, creates if not found. Non-recursive search.

**Folder Removal:**
```csharp
public static bool RemoveFolder(this DevelopPlan plan, DevelopFolder folder, bool recursive = false)
```
Removes folder from disk (with optional recursive delete) and from plan. Returns `true` if removed.

**Search:**
```csharp
public static bool TryFindFolder(this DevelopPlan plan, string folderId, out DevelopFolder? folder, bool recursive = true)

public static bool TryFindFolder(this DevelopFolder folder, string folderId, out DevelopFolder? model, bool recursive = true)

public static DevelopFolder? GetFolderOfFile(this DevelopPlan plan, DevelopFile file)

public static DevelopFolder? GetFolderOfFile(this DevelopPlan plan, DevelopFolder folder, DevelopFile file)
```
Search for folder by ID or find which folder contains a file. Recursive by default.

---

## Options

### DevelopOptions

```csharp
public sealed class DevelopOptions
```

**Namespace:** `Cyrena.Coding.Options`

Configuration constants for the developer SDK.

**Constants:**
```csharp
public const string AssistantModeId = "developer";
public const string BuilderId = "dev.builder-id";
[Obsolete]
//Removed, uses new ChatConfiguration.WorkingDirectory.
public const string RootDirectory = "dev.root-dir";
```

---

## Usage Patterns

### Loading a Plan
```csharp
IDevelopPlanService planService = serviceProvider.GetRequiredService<IDevelopPlanService>();
DevelopPlan plan = planService.Plan;
```

### Creating Files
```csharp
DevelopFile file = plan.CreateFile("models_UserModel", "UserModel.cs", "namespace App { }");

DevelopFolder src = plan.CreateFolder("src", "src");
DevelopFile srcFile = plan.CreateFile(plan, src, "models_UserModel", "UserModel.cs", "namespace App { }");
```

### Reading File Content
```csharp
if (plan.TryReadFileLines(file, out DevelopFileLines? lines))
{
    string line0 = lines.Lines[0];
    // lines.ToString() reconstructs with Windows line endings
}
```

### Modifying a Single Line
```csharp
if (plan.TryWriteFileLine(file, 5, "new content", out var updated))
{
    // File updated on disk and lines returned
}
```

### Searching Files
```csharp
if (plan.TryFindFile("models_UserModel", out var found))
{
    // Found by ID
}

if (plan.TryFindFileByName("UserModel.cs", out var found2))
{
    // Found by name (case-insensitive)
}
```

### Indexing Project Files
```csharp
plan.IndexFiles("cs", "models_", readOnly: true);
plan.IndexFiles(plan.Folders[0], "cs", "services_", readOnly: false);
```

### Version Control Backup
```csharp
IVersionControl vc = serviceProvider.GetRequiredService<IVersionControl>();

if (plan.TryReadFileContent(file, out var content))
{
    vc.Backup(content);
}

if (vc.HasBackup(file.Id))
{
    var backups = vc.GetBackups(file.Id);
}
```

### Folder Management
```csharp
DevelopFolder folder = plan.GetOrCreateFolder("models", "Models");

var containingFolder = plan.GetFolderOfFile(file);
```

---

## Integration

### Service Registration Pattern
Services are expected to be registered through the `ICodeBuilder.ConfigureAsync()` method which receives a `CyrenaKernelBuilder` for dependency injection setup.

### Plan Persistence
- **Save:** `DevelopPlan.Save(plan)` writes to `.cyrena/plan`
- **Load:** `DevelopPlan.TryLoadFromDirectory(dir, out plan)` reads from `.cyrena/plan`
- Plan objects contain `[JsonIgnore]` properties for runtime paths

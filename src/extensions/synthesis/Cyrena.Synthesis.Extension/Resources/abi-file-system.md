# IFileSystemAbi

Sandboxed filesystem capability exposed through the runtime capability context.

Access through:

```fsharp
open Cyrena.Synthesis.Contracts
```

```fsharp
let files = ctx.GetService<IFileSystemAbi>()
```

OR

```fsharp
let files = ctx.GetRequiredService<IFileSystemAbi>()
```

Provides filesystem operations scoped to the approved runtime sandbox.

Available methods:

```fsharp
files.ReadText(path)
files.WriteText(path, content)
files.Exists(path)
files.Delete(path)
files.ListFiles(path)
files.ListDirectories(path)
files.CreateDirectory(path)
files.DeleteDirectory(path)
```

Paths are relative to the approved sandbox scope.

Never construct absolute paths. Never prepend directory names derived from capability names, argument values, or any other source. Pass the path exactly as intended — the runtime resolves it within the sandbox automatically.

Correct:

```fsharp
files.WriteText("notes.txt", content)
files.ReadText("entries/2024-01-01.json")
files.ListFiles("logs")
```

Incorrect — do not do this:

```fsharp
files.WriteText("/food/notes.txt", content)       // absolute path
files.WriteText("food/food/notes.txt", content)   // duplicated directory
files.WriteText(capabilityId + "/notes.txt", content) // constructing paths from runtime values
```

Example:

```fsharp
open Cyrena.Synthesis.Contracts

let main (ctx: ICapabilityExecutionContext) =
    let files = ctx.GetRequiredService<IFileSystemAbi>()

    files.WriteText("notes.txt", "hello")
```
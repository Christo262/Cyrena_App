# ICapabilityExecutionContext

Primary runtime context exposed to Dynamic Capabilities.

Every Dynamic Capability receives an `ICapabilityExecutionContext` through the runtime entrypoint.

Required entrypoint:

```fsharp
let main (ctx: ICapabilityExecutionContext) =
```

The execution context provides:

* structured argument access
* structured runtime logging
* structured execution results
* access to approved ABI services

Available properties:

```fsharp
ctx.Args
ctx.Log
ctx.Result
```

Available methods:

```fsharp
ctx.GetService<T>()
ctx.GetRequiredService<T>()
```

Use `ctx.Args` for all argument access.

Example:

```fsharp
let food = ctx.Args.GetString("food")
```

Use `ctx.Log` for runtime output and diagnostics.

Example:

```fsharp
ctx.Log.Info("Processing food entry")
```

Use `ctx.Result` for structured execution results returned to the runtime.

Example:

```fsharp
ctx.Result.Text("summary", "Food entry added")
ctx.Result.Boolean("success", true)
```

Use `GetRequiredService<T>()` for required ABI services.

Example:

```fsharp
let files = ctx.GetRequiredService<IFileSystemAbi>()
```

Use `GetService<T>()` only for optional ABI services.

Example:

```fsharp
let optionalService = ctx.GetService<IMyOptionalAbi>()
```

Only use ABI services exposed through registered ABI descriptors.

Example:

```fsharp
open Cyrena.Synthesis.Contracts

let main (ctx: ICapabilityExecutionContext) =
    let files = ctx.GetRequiredService<IFileSystemAbi>()

    let path = ctx.Args.GetString("path")

    if files.Exists(path) then
        let text = files.ReadText(path)

        ctx.Log.Info("File loaded successfully")

        ctx.Result.Boolean("success", true)
        ctx.Result.Text("content", text)
    else
        ctx.Log.Error("File not found")
        ctx.R
```

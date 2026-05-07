# ICapabilityLogger

Structured runtime logging for Dynamic Capabilities.

Access through:

```fsharp
ctx.Log
```

Dynamic Capabilities must use `ctx.Log` for all runtime output, diagnostics, warnings, and errors.

Do not use:

```fsharp
Console.WriteLine
Debug.WriteLine
Trace.WriteLine
```

Available methods:

```fsharp
ctx.Log.Debug("message")
ctx.Log.Info("message")
ctx.Log.Warn("message")
ctx.Log.Error("message")
ctx.Log.Error("message", exception)
```

Use `Debug` for detailed diagnostic information:

```fsharp
ctx.Log.Debug("Loading configuration file")
```

Use `Info` for normal execution progress:

```fsharp
ctx.Log.Info("Food entry added successfully")
```

Use `Warn` for recoverable or unexpected conditions:

```fsharp
ctx.Log.Warn("Calories value was missing, defaulting to 0")
```

Use `Error` for failures affecting execution:

```fsharp
ctx.Log.Error("Failed to save food entry")
```

Use exception overloads when exception details are available:

```fsharp
try
    files.WriteText("notes.txt", "hello")
with ex ->
    ctx.Log.Error("Failed to write file", ex)
```

Formatted logging should use `sprintf`:

```fsharp
ctx.Log.Info(sprintf "Added %s to %s" food meal)
```

Example:

```fsharp
open Cyrena.Synthesis.Contracts

let main (ctx: ICapabilityExecutionContext) =
    let food = ctx.Args.GetString("food")

    ctx.Log.Debug("Starting food entry processing")

    if String.IsNullOrWhiteSpace(food) then
        ctx.Log.Error("Food name is required")
    else
        ctx.Log.Info(sprintf "Food entry added: %s" food)
```

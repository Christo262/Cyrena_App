# ICapabilityResultWriter

Structured result output for Dynamic Capabilities.

Access through:

```fsharp
ctx.Result
```

Use `ctx.Result` to return structured data back to the runtime and AI model.

`ctx.Log` is used for runtime logging and diagnostics.

`ctx.Result` is used for structured execution results.

Available methods:

```fsharp
ctx.Result.Text("key", "value")
ctx.Result.Number("key", 123.0)
ctx.Result.Boolean("key", true)
ctx.Result.Json("key", value)
ctx.Result.Error("code", "message")
```

Available property:

Use `Text` for normal string results:

```fsharp
ctx.Result.Text("summary", "Food entry added successfully")
```

Use `Number` for numeric results:

```fsharp
ctx.Result.Number("totalCalories", 450.0)
```

Use `Boolean` for true/false results:

```fsharp
ctx.Result.Boolean("success", true)
```

Use `Json` for structured objects or collections:

```fsharp
type FoodEntry = {
    Food: string
    Calories: int
}

let entry = {
    Food = "Banana"
    Calories = 105
}

ctx.Result.Json("entry", entry)
```

Use `Error` for structured execution errors:

```fsharp
ctx.Result.Error("file_not_found", "The requested file does not exist")
```

Results are returned to the runtime after execution completes.

Use result keys that are:

* short
* descriptive
* lowercase when possible
* stable across executions

Good examples:

```text
summary
success
totalCalories
entry
entries
filePath
```

Example:

```fsharp
open Cyrena.Synthesis.Contracts

type FoodEntry = {
    Food: string
    Calories: int
}

let main (ctx: ICapabilityExecutionContext) =
    let food = ctx.Args.GetString("food")
    let calories = ctx.Args.GetInt32("calories")

    let entry = {
        Food = food
        Calories = calories
    }

    ctx.Log.Info("Food entry added")

    ctx.Result.Boolean("success", true)
    ctx.Result.Text("summa
```

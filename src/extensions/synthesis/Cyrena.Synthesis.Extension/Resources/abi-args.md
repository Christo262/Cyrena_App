# ICapabilityArgs

Structured argument access for Dynamic Capabilities.

Access through:

```fsharp
ctx.Args
```

Arguments are accessed by name.

Available methods:

```fsharp
ctx.Args.GetString("name")
ctx.Args.GetInt32("name")
ctx.Args.GetBoolean("name")
ctx.Args.GetDouble("name")
ctx.Args.GetJson<T>("name")
ctx.Args.Has("name")
ctx.Args.GetRaw("name")
```

Available properties:

```fsharp
ctx.Args.Names
ctx.Args.Count
```

Use `Has` when an argument is optional:

```fsharp
let date =
    if ctx.Args.Has("date") then
        ctx.Args.GetString("date")
    else
        "today"
```

Use typed accessors for normal argument values:

```fsharp
let food = ctx.Args.GetString("food")
let calories = ctx.Args.GetInt32("calories")
let enabled = ctx.Args.GetBoolean("enabled")
let amount = ctx.Args.GetDouble("amount")
```

Use `GetRaw` when the original unconverted string value is needed:

```fsharp
let rawDate = ctx.Args.GetRaw("date")
```

Use `GetJson<T>` for structured JSON arguments:

```fsharp
type FoodEntryInput = {
    Food: string
    Quantity: string
    Calories: int
}

let input = ctx.Args.GetJson<FoodEntryInput>("entry")
```

Example:

```fsharp
open Cyrena.Synthesis.Contracts

let main (ctx: ICapabilityExecutionContext) =
    let food = ctx.Args.GetString("food")
    let meal = ctx.Args.GetString("meal")

    let calories =
        if ctx.Args.Has("calories") then
            ctx.Args.GetInt32("calories")
        else
            0

    ctx.Log.Info(sprintf "Food: %s, Meal: %s, Calories: %d" food meal calories)
```

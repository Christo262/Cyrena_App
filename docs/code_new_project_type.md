# Supporting Different Projects

## Overview

To support a new project type, create a Razor Class Library that contains the services, components, and prompts required for that project.

Use the `Cyrena.ArduinoIDE` project as a structural reference.

Each project type is implemented as a self-contained extension layer that teaches Cyréna how to understand, configure, and index that ecosystem.

> **Note:** .NET project support works differently and requires additional implementation steps. See the .NET Support documentation.

---

## Development Plan

Every project type must define a strict development plan. The model should not be able to deviate from it — the less the model has to decide, the more stable the assistance becomes.

Arduino IDE projects are a useful illustration because their structure is intentionally simple:

```
Root/
├── sketch.ino
├── some.h
└── some.cpp
```

This flat layout makes defining a development plan straightforward.

**`Cyrena.Developer.Contracts.ICodeBuilder`** must be implemented to configure a new supported project type:

```csharp
internal class ArduinoIDECodeBuilder : ICodeBuilder
{
    private readonly IKernelController _kernel;

    public ArduinoIDECodeBuilder(IKernelController kernel)
    {
        _kernel = kernel;
    }

    public string Id => ArduinoOptions.BuilderId;

    public Task<DevelopPlan> ConfigureAsync(DevelopOptions options)
    {
        options.Plugins.AddFromType<Arduino>();
        options.KernelBuilder.AddStartupTask<PromptStartupTask>();

        var plan = new DevelopPlan(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
        plan.IndexFiles("ino", "ino_");
        plan.IndexFiles("h", "h_");
        plan.IndexFiles("cpp", "cpp_");

        return Task.FromResult(plan);
    }

    public Task DeleteAsync(ChatConfiguration config)
    {
        return Task.CompletedTask;
    }

    public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
    {
        var dialog = services.GetRequiredService<DialogService>();

        var rf = await dialog.ShowModal<Configure>(new ResultDialogOption()
        {
            Title = "Arduino IDE",
            Size = Size.Medium,
            ComponentParameters = new()
            {
                { nameof(Configure.Model), config }
            },
            ButtonYesText = "Save",
            ButtonNoText = "Cancel",
        });

        if (rf == DialogResult.Yes)
            await _kernel.UpdateAsync(config, true);
    }
}
```

This service configures the prompts, registers the functions the model can call, and handles any additional UI or service setup required for that project's kernel instance.
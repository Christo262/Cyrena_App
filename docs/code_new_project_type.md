# Supporting Different Projects

## Overview

To support a new project type, create a Razor Class Library that contains the services, components, and prompts required for that project.  
Use the `Cyrena.ArduinoIDE` project as a structural reference.

Each project type is implemented as a self-contained extension layer that teaches Cyréna how to understand, configure, and index that ecosystem.

**Please Note**: .NET Project support works slightly different and requires additional implements. [.NET Support](./dotnet-support.md).

---

## Develop Plan

Each project type must have a strict development plan. The AI should not be allowed to deviate from it, the
less the AI has to decide, the more stable the assistance becomes.

Arduino IDE projects typically are incredibly simple:
- *Root Dir*
	- sketch.ino
	- some.h
	- some.cpp
	
This makes creating a Develop Plan easier.

**Cyrena.Developer.Contracts.ICodeBuilder**: This interface must be implemented in order to configure
a new supported project.

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
                    {nameof(Configure.Model), config }
                },
                ButtonYesText = "Save",
                ButtonNoText = "Cancel",
            });
            if (rf == DialogResult.Yes)
                await _kernel.UpdateAsync(config, true);
        }
    }
```
This servicecs configures the prompt, additional functions required to adhere to the plan and any additional UI configs or services that is required on the instance of Kernel.


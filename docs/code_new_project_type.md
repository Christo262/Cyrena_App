# Supporting Different Projects

## Overview

To support a new project type, create a Razor Class Library that contains the services, components, and prompts required for that project.  
Use the `Cyrena.Blazor` project as a structural reference.

Each project type is implemented as a self-contained extension layer that teaches Cyréna how to understand, configure, and index that ecosystem.

---

## 1. Project Configurator

The configurator defines how a project type is created, edited, initialized, and integrated into Cyréna.  
See: `Cyrena.Contracts.IProjectConfigurator`.

Below is a simplified example for a .NET class library:

```csharp
namespace Cyrena.ClassLibrary.Services
{
    internal class LibraryConfigurator : IProjectConfigurator
    {
        private readonly DialogService _dialog;
        private readonly IStore<Project> _store;

        public LibraryConfigurator(DialogService dialog, IStore<Project> store)
        {
            _dialog = dialog;
            _store = store;
        }

        public string ProjectType => "dotnet-class-lib"; // MUST be unique
        public string Name => "Class Library";
        public string? Description => "Build a .NET class library";
        public string? Icon => "bi bi-collection"; // Bootstrap Icons
        public string Category => ".NET C#";

        // Creates a new project instance
        public async Task<bool> CreateNewAsync()
        {
            var project = new ClassLibraryProject();

            var rf = await _dialog.ShowModal<ClassLibraryConfig>(new ResultDialogOption()
            {
                Title = "Class Library",
                Size = BootstrapBlazor.Components.Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", project }
                }
            });

            if (rf == DialogResult.Yes)
            {
                await _store.AddAsync(project);
                return true;
            }

            return false;
        }

        // Edits an existing project configuration
        public async Task EditAsync(Project project)
        {
            var model = new ClassLibraryProject(project);

            var rf = await _dialog.ShowModal<ClassLibraryConfig>(new ResultDialogOption()
            {
                Title = "Class Library",
                Size = BootstrapBlazor.Components.Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });

            if (rf == DialogResult.Yes)
                await _store.UpdateAsync(project);
        }

        // Initializes indexing, plugins, and system prompt
        public Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder)
        {
            var plan = new ProjectPlan(builder.Project.RootDirectory);

            plan.IndexFiles("csproj", "app_", true);

            var contracts = plan.GetOrCreateFolder("contracts", "Contracts");
            plan.IndexFiles(contracts, "cs", "contracts_");

            var models = plan.GetOrCreateFolder("models", "Models");
            plan.IndexFiles(models, "cs", "models_");

            var services = plan.GetOrCreateFolder("services", "Services");
            plan.IndexFiles(services, "cs", "services_");

            var extensions = plan.GetOrCreateFolder("extensions", "Extensions");
            plan.IndexFiles(extensions, "cs", "extensions_");

            var options = plan.GetOrCreateFolder("options", "Options");
            plan.IndexFiles(options, "cs", "options_");

            ProjectPlan.Save(plan);

            var prompt = File.ReadAllText("./class_lib_prompt.md");
            builder.KernelHistory.AddSystemMessage(prompt);

            builder.Plugins.AddFromType<DefaultStructurePlugin>();
            builder.Plugins.AddFromType<DotnetActions>();

            builder.AddEventHandler<FileCreatedEvent, LibProjectFileWatcher>();
            builder.AddEventHandler<FileDeletedEvent, LibProjectFileWatcher>();
            builder.AddEventHandler<FileRenamedEvent, LibProjectFileWatcher>();

            return Task.FromResult(plan);
        }
    }
}
```

---

## 2. File Change Event Handling

Due to OS-level watcher limitations, file filtering cannot always be perfect.  
Instead, event handlers validate whether a file is relevant before updating the UI.

```csharp
using Cyrena.ClassLibrary.Extensions;
using Cyrena.Contracts;
using Cyrena.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.ClassLibrary.Services
{
    internal class LibProjectFileWatcher :
        IEventHandler<FileCreatedEvent>,
        IEventHandler<FileDeletedEvent>,
        IEventHandler<FileRenamedEvent>
    {
        private readonly IDeveloperContext _ctx;

        public LibProjectFileWatcher(IDeveloperContext ctx)
        {
            _ctx = ctx;
        }

        public Task HandleAsync(FileCreatedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();

            return Task.CompletedTask;
        }

        public Task HandleAsync(FileDeletedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();

            return Task.CompletedTask;
        }

        public Task HandleAsync(FileRenamedEvent e, CancellationToken ct)
        {
            if (_ctx.IsTrackedFile(e.FullPath))
                _ctx.Kernel.Services.GetRequiredService<IDeveloperWindow>().FilesChanged();

            return Task.CompletedTask;
        }
    }
}
```

---

## 3. Prompt Design

Each project type must provide a dedicated prompt file.

Do not hardcode prompts in source.  
Always load from a `.md` file to allow easy iteration and customization.

Example:

```
Cyrena.ClassLibrary/class_lib_prompt.md
```

Prompts should enforce strict structure.  
The less the model has to infer architecture, the more reliable the output.

Focus the prompt on:

- structure rules
- conventions
- allowed operations
- implementation scope

Avoid ambiguity.

---

## 4. Registering the Project Type

Register the configurator using a builder extension:

```csharp
public static class CyrenaBuilderExtensions
{
    public static CyrenaBuilder AddClassLibraryDevelopment(this CyrenaBuilder builder)
    {
        builder.Services.AddScoped<IProjectConfigurator, LibraryConfigurator>();
        return builder;
    }
}
```

Then enable it in:

```
Cyrena.Desktop.Program.cs
```

---

At this point the project type is fully integrated:

✔ configurator  
✔ indexing plan  
✔ prompt  
✔ file event handling  
✔ UI integration  
✔ builder registration

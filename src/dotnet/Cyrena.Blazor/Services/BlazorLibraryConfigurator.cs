using BootstrapBlazor.Components;
using Cyrena.Blazor.Components.Shared;
using Cyrena.Blazor.Extensions;
using Cyrena.Blazor.Plugins;
using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Net.Models;
using Cyrena.Net.Plugins;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Blazor.Services
{
    internal class BlazorLibraryConfigurator : IProjectConfigurator
    {
        private readonly DialogService _dialog;
        private readonly IStore<Project> _store;
        public BlazorLibraryConfigurator(DialogService dialog, IStore<Project> store)
        {
            _dialog = dialog;
            _store = store;
        }
        public string ProjectType => "blazor-class-lib";
        public string Name => "Blazor Component Library";
        public string? Description => "Build a reusable blazor component library";
        public string? Icon => "bi bi-journal-richtext";
        public string Category => ".NET C#";

        public async Task<bool> CreateNewAsync()
        {
            var project = new DotnetProject(ProjectType);
            var rf = await _dialog.ShowModal<BlazorConfig>(new ResultDialogOption()
            {
                Title = "Blazor Library",
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

        public async Task EditAsync(Project project)
        {
            var model = new DotnetProject(project, ProjectType);
            var rf = await _dialog.ShowModal<BlazorConfig>(new ResultDialogOption()
            {
                Title = "Blazor Library",
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

        public Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder)
        {
            var plan = builder.LoadBlazorDefaultPlan();
            var prompt = File.ReadAllText("./blazor_lib_prompt.md");
            builder.KernelHistory.AddSystemMessage(prompt);
            builder.Plugins.AddFromType<BlazorCreatePlugin>();
            builder.Plugins.AddFromType<DefaultStructurePlugin>();
            builder.Plugins.AddFromType<DotnetActions>();
            builder.AddEventHandler<FileCreatedEvent, BlazorProjectFileWatcher>();
            builder.AddEventHandler<FileDeletedEvent, BlazorProjectFileWatcher>();
            builder.AddEventHandler<FileRenamedEvent, BlazorProjectFileWatcher>();
            return Task.FromResult(plan);
        }
    }
}

/*
 * - wwwroot
	- css
		- styles.css
	- js
		- site.js
- Components
	- Layout
		- MainLayout.razor
	- Pages
		- Index.razor //Example
	- Shared
		- NavMenu.razor
	- _Imports.razor
	- App.razor
	- Routes.razor
- Contracts
	- IMyService.cs
- Models
	- MyModel.cs
- Services
	- MyService.cs
- Extensions
	- MyServiceExtensions.cs
	- MyModelExtensions.cs
- Options
    - SomeOptions.cs
 */
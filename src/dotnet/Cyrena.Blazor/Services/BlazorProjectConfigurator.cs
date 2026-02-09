using BootstrapBlazor.Components;
using Cyrena.Blazor.Components.Shared;
using Cyrena.Blazor.Extensions;
using Cyrena.Blazor.Plugins;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Net.Models;
using Cyrena.Net.Plugins;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;

namespace Cyrena.Blazor.Services
{
    internal class BlazorProjectConfigurator : IProjectConfigurator
    {
        private readonly DialogService _dialog;
        private readonly IStore<Project> _store;
        public BlazorProjectConfigurator(DialogService dialog, IStore<Project> store)
        {
            _dialog = dialog;
            _store = store;
        }

        public string ProjectType => "blazor-app";
        public string Name => "Blazor App";
        public string? Description => "Build a Blazor Server or WASM application.";
        public string? Icon => "bi bi-hdd-stack";
        public string Category => ".NET C#";

        public Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder)
        {
            var plan = builder.LoadBlazorDefaultPlan();
            var prompt = File.ReadAllText("./blazor_prompt.md");
            builder.KernelHistory.AddSystemMessage(prompt);
            builder.Plugins.AddFromType<BlazorCreatePlugin>();
            builder.Plugins.AddFromType<DefaultStructurePlugin>();
            builder.Plugins.AddFromType<DotnetActions>();
            return Task.FromResult(plan);
        }

        public async Task<bool> CreateNewAsync()
        {
            var project = new DotnetProject(ProjectType);
            var rf = await _dialog.ShowModal<BlazorConfig>(new ResultDialogOption()
            {
                Title = "Blazor App",
                Size = BootstrapBlazor.Components.Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", project }
                }
            });
            if(rf == DialogResult.Yes)
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
                Title = "Blazor App",
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
    }
}

/*
- wwwroot
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
- Program.cs //entry file
- appsettings.json
- appsettings.Development.json
 */
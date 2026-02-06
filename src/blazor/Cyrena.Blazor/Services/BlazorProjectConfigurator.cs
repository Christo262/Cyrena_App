using BootstrapBlazor.Components;
using Microsoft.SemanticKernel;
using Cyrena.Blazor.Extensions;
using Cyrena.Blazor.Plugins;
using Cyrena.Blazor.Components.Shared;
using Cyrena.Blazor.Models;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;

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

        public Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder)
        {
            var plan = builder.LoadBlazorDefaultPlan();
            var prompt = File.ReadAllText("./blazor_prompt.txt");
            builder.KernelHistory.AddSystemMessage(prompt);
            builder.Plugins.AddFromType<BlazorCreatePlugin>();
            builder.Plugins.AddFromType<BlazorActions>();
            return Task.FromResult(plan);
        }

        public async Task<bool> CreateNewAsync()
        {
            var project = new BlazorProject();
            var rf = await _dialog.ShowModal<BlazorServerConfig>(new ResultDialogOption()
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
            var model = new BlazorProject(project);
            var rf = await _dialog.ShowModal<BlazorServerConfig>(new ResultDialogOption()
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

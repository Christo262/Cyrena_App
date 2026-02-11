using BootstrapBlazor.Components;
using Cyrena.ClassLibrary.Components.Shared;
using Cyrena.ClassLibrary.Extensions;
using Cyrena.ClassLibrary.Models;
using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Net.Plugins;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

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
        public string ProjectType => "dotnet-class-lib";
        public string Name => "Class Library";
        public string? Description => "Build a .NET class library";
        public string? Icon => "bi bi-collection";
        public string Category => ".NET C#";

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
                await _store.UpdateAsync(model);
        }

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

            builder.Plugins.AddFromType<DotnetCreate>();
            builder.Plugins.AddFromType<Dotnet>();

            builder.AddEventHandler<FileCreatedEvent, LibProjectFileWatcher>();
            builder.AddEventHandler<FileDeletedEvent, LibProjectFileWatcher>();
            builder.AddEventHandler<FileRenamedEvent, LibProjectFileWatcher>();

            return Task.FromResult(plan);
        }
    }
}

/*
 * Structure:
 *      Contracts: interfaces
 *      Models : data classes
 *      Services: implementations
 *      Extensions: extension methods
 *      Options: classes for config
 */

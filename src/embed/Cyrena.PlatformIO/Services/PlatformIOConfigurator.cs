using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.PlatformIO.Services
{
    internal class PlatformIOConfigurator : IProjectConfigurator
    {
        private readonly DialogService _dialog;
        private readonly IStore<Project> _store;
        public PlatformIOConfigurator(DialogService dialog, IStore<Project> store)
        {
            _dialog = dialog;
            _store = store;
        }

        public string ProjectType => "platformio-ide";
        public string Name => "PlatformIO";
        public string Category => "Embedded";
        public string? Description => "Build firmware using PlatformIO with arduino and espidf frameworks";
        public string? Icon => "bi bi-cpu";

        public async Task<bool> CreateNewAsync()
        {
            var project = new Project() { Type = ProjectType };
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "PlatformIO Project",
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

        public async Task EditAsync(Project model)
        {
            model.Type = ProjectType;
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "PlatformIO Project",
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

        public async Task<ProjectPlan> InitializeAsync(IDeveloperContextBuilder builder)
        {
            ProjectPlan plan;

            if (!ProjectPlan.TryLoadFromDirectory(builder.Project.RootDirectory, out plan))
                plan = new ProjectPlan(builder.Project.RootDirectory);

            plan.IndexFiles("ini", "ini_", true);

            if (!plan.TryFindFile("ini_platformio", out var pio, false))
            {
                await _dialog.ShowModal("Error", "Unable to locate platformio.ini file");
                throw new InvalidOperationException("platformio.ini not found");
            }

            var environments = PlatformIOEnvironment.Parse(
                Path.Combine(builder.Project.RootDirectory, pio!.RelativePath));

            if (!environments.Any())
            {
                await _dialog.ShowModal("Error", "No environments defined in platformio.ini");
                throw new InvalidOperationException("No environments defined in platformio.ini");
            }

            IEnvironmentController environmentController =
                new EnvironmentController(environments);

            if (environments.Count > 1)
            {
                await _dialog.ShowModal<EnvironmentSelector>(new ResultDialogOption()
                {
                    Title = "Select Environment",
                    ButtonNoText = "Cancel",
                    ButtonYesText = "Done",
                    Size = Size.Medium,
                    ComponentParameters = new()
                        {
                            { "Controller", environmentController }
                        }
                });

                if (environmentController.Current == null)
                    throw new InvalidOperationException("Environment selection cancelled");
            }

            plan.IndexPlatformIODefaultPlan();

            if (environmentController.Current!.Framework?
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                plan.IndexPlatformIOEspIdf();

                var envName = environmentController.Current.Name.Replace("env:", "");
                var sdkName = $"sdkconfig.{envName}";
                var sdkPath = Path.Combine(builder.Project.RootDirectory, sdkName);

                if (File.Exists(sdkPath) && !plan.TryFindFileByName(sdkName, out _))
                {
                    plan.Files.Add(new ProjectFile()
                    {
                        Id = "sdkconfig",
                        Name = sdkName,
                        RelativePath = sdkName,
                        ReadOnly = true
                    });
                }
            }
            var prompt = File.ReadAllText("./platformio_prompt.md");
            builder.KernelHistory.AddSystemMessage(prompt);
            builder.Services.AddSingleton<IEnvironmentController>(environmentController);
            builder.Plugins.AddFromType<StandardStructurePlugin>();

            builder.AddEventHandler<FileCreatedEvent, PioFileWatcher>();
            builder.AddEventHandler<FileDeletedEvent, PioFileWatcher>();
            builder.AddEventHandler<FileRenamedEvent, PioFileWatcher>();

            return plan;
        }

    }
}

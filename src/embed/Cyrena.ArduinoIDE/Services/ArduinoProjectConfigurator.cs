using BootstrapBlazor.Components;
using Cyrena.ArduinoIDE.Components.Shared;
using Cyrena.ArduinoIDE.Models;
using Cyrena.ArduinoIDE.Plugins;
using Cyrena.Contracts;
using Cyrena.Events;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;
using System.Text;

namespace Cyrena.ArduinoIDE.Services
{
    internal class ArduinoProjectConfigurator : IProjectConfigurator
    {
        private readonly DialogService _dialog;
        private readonly IStore<Project> _store;
        public ArduinoProjectConfigurator(DialogService dialog, IStore<Project> store)
        {
            _dialog = dialog;
            _store = store;
        }

        public string ProjectType => ArduinoProject.TypeId;
        public string Name => "Arduino";
        public string Category => "Embedded";
        public string? Description => "Develop a Arduino sketch with the Arduino IDE";
        public string? Icon => "bi bi-cpu";

        public async Task<bool> CreateNewAsync()
        {
            var project = new ArduinoProject();
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Arduino Project",
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
            var model = new ArduinoProject(project);
            var rf = await _dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Arduino Project",
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
            if (!ProjectPlan.TryLoadFromDirectory(builder.Project.RootDirectory, out var plan))
                plan = new ProjectPlan(builder.Project.RootDirectory);

            plan.IndexFiles("ino", "ino_");
            plan.IndexFiles("cpp", "cpp_");
            plan.IndexFiles("h", "h_");

            builder.Plugins.AddFromType<Arduino>();
            var prompt = File.ReadAllText("./arduino_ide_prompt.md");
            var boardCtx = new StringBuilder();
            boardCtx.AppendLine($"Board: {builder.Project.Properties[ArduinoProject.BoardProp]}");
            boardCtx.AppendLine($"RAM: {builder.Project.Properties[ArduinoProject.RamProp]}");
            boardCtx.AppendLine($"Clock: {builder.Project.Properties[ArduinoProject.ClockProp]}");
            prompt = prompt.Replace("{BOARD_CONTEXT}", boardCtx.ToString());

            builder.KernelHistory.AddSystemMessage(prompt);

            builder.AddEventHandler<FileCreatedEvent, ArduinoProjectFileWatcher>();
            builder.AddEventHandler<FileDeletedEvent, ArduinoProjectFileWatcher>();
            builder.AddEventHandler<FileRenamedEvent, ArduinoProjectFileWatcher>();

            return Task.FromResult(plan);
        }
    }
}

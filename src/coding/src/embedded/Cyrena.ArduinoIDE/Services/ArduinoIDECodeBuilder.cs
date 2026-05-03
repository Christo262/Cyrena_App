using BootstrapBlazor.Components;
using Cyrena.ArduinoIDE.Components.Shared;
using Cyrena.ArduinoIDE.Options;
using Cyrena.ArduinoIDE.Plugins;
using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text;

namespace Cyrena.ArduinoIDE.Services
{
    internal class ArduinoIDECodeBuilder : ICodeBuilder
    {
        private readonly IKernelController _kernel;
        public ArduinoIDECodeBuilder(IKernelController kernel)
        {
            _kernel = kernel;
        }

        public string Id => ArduinoOptions.BuilderId;

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            options.Plugins.AddFromType<Arduino>();
            
            var plan = new DevelopPlan(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            plan.IndexFiles("ino", "ino_");
            plan.IndexFiles("h", "h_");
            plan.IndexFiles("cpp", "cpp_");

            var prompt = Resources.Read(typeof(ArduinoIDECodeBuilder).Assembly, "Cyrena.ArduinoIDE.Resources.prompt.md");
            var sb = new StringBuilder();
            sb.AppendLine($"Board: {options.ChatConfiguration[ArduinoOptions.BoardId]}");
            sb.AppendLine($"RAM: {options.ChatConfiguration[ArduinoOptions.Ram]}");
            sb.AppendLine($"Clock: {options.ChatConfiguration[ArduinoOptions.Clock]}");
            prompt = prompt.Replace("{BOARD_CONTEXT}", sb.ToString());
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.Services.AddSingleton<IDevelopPlanIndexer, DevelopPlanIndexer>();
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
}

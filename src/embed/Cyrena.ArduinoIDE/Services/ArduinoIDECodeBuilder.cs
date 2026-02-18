using BootstrapBlazor.Components;
using Cyrena.ArduinoIDE.Components.Shared;
using Cyrena.ArduinoIDE.Options;
using Cyrena.ArduinoIDE.Plugins;
using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
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

    internal class PromptStartupTask : IStartupTask
    {
        private readonly IChatMessageService _chat;
        private readonly IChatConfigurationService _config;
        public PromptStartupTask(IChatMessageService chat, IChatConfigurationService config)
        {
            _chat = chat;
            _config = config;
        }

        public int Order => 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var prompt = ReadPrompt();
            var sb = new StringBuilder();
            sb.AppendLine($"Board: {_config.Config[ArduinoOptions.BoardId]}");
            sb.AppendLine($"RAM: {_config.Config[ArduinoOptions.Ram]}");
            sb.AppendLine($"Clock: {_config.Config[ArduinoOptions.Clock]}");
            prompt = prompt.Replace("{BOARD_CONTEXT}", sb.ToString());
            await _chat.AddSystemMessage(prompt);
        }

        private string ReadPrompt()
        {
            var assembly = typeof(PromptStartupTask).Assembly;
            var resourceName = "Cyrena.ArduinoIDE.arduino-ide-prompt.md";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Options;
using Cyrena.PlatformIO.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.PlatformIO.Services
{
    internal class PlatformIOBuilder : ICodeBuilder
    {
        private readonly IKernelController _kernel;
        public PlatformIOBuilder(IKernelController kernel)
        {
            _kernel = kernel;
        }

        public string Id => PlatformIOOptions.BuilderId;

        public Task<DevelopPlan> ConfigureAsync(DevelopOptions options)
        {
            var plan = new DevelopPlan(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            plan.IndexFiles("ini", "ini_", true);
            if (!plan.TryFindFile("ini_platformio", out var pio, false))
                throw new InvalidOperationException("platformio.ini not found");
            var environments = PlatformIOEnvironment.Parse(options.ChatConfiguration[PlatformIOOptions.IniFile] ?? throw new NullReferenceException("platformio.ini not set"));
            if (!environments.Any())
                throw new InvalidOperationException("No environments defined in platformio.ini");
            IEnvironmentController environmentController =
               new EnvironmentController(environments);
            environmentController.SetCurrentEnvironment(environments[0].Name);
            plan.IndexPlatformIODefaultPlan();

            options.Services.AddSingleton<IEnvironmentController>(environmentController);
            options.Plugins.AddFromType<Cyrena.PlatformIO.Plugins.Platform>();
            options.KernelBuilder.AddStartupTask<PromptStartupTask>();
            options.KernelBuilder.AddToolbarComponent<Cyrena.PlatformIO.Components.Shared.Toolbar>(ToolbarAlignment.Start);
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
                Title = "PlatformIO",
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
        private readonly IDevelopPlanService _plan;
        public PromptStartupTask(IChatMessageService chat, IChatConfigurationService config, IDevelopPlanService plan)
        {
            _chat = chat;
            _config = config;
            _plan = plan;
        }

        public int Order => 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var prompt = ReadPrompt();
            await _chat.AddSystemMessage(prompt);
            
        }

        private string ReadPrompt()
        {
            var assembly = typeof(PromptStartupTask).Assembly;
            var resourceName = "Cyrena.PlatformIO.platformio-prompt.md";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
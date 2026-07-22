using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using MudBlazor;
using System.Text;
using System.Threading.Tasks;

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

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var plan = new DevelopPlan(options.ChatConfiguration.WorkingDirectory!);
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
            options.Plugins.AddFromType<Platform>();
            var prompt = Resources.Read(typeof(PlatformIOBuilder).Assembly, "Cyrena.PlatformIO.Resources.prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.AddToolbarComponent<Cyrena.PlatformIO.Components.Shared.Toolbar>(ToolbarAlignment.Start);
            options.Services.AddSingleton<IDevelopPlanIndexer, DevelopPlanIndexer>();
            return Task.FromResult(plan);
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialogService = services.GetRequiredService<IDialogService>();
            var parameters = new DialogParameters<Configure>
            {
                { x => x.Model, config }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small };
            var dialog = await dialogService.ShowAsync<Configure>("Structured PlatformIO", parameters, options);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
                await _kernel.UpdateAsync(config, true);
        }
    }
}
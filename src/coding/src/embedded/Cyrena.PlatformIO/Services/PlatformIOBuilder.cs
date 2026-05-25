using Cyrena.PlatformIO.Components.Shared;
using Cyrena.PlatformIO.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Extensions;
using Cyrena.Models;
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
            plan.IndexFiles("ini", "ini_");
            plan.IndexFiles("h", "h_");
            plan.IndexFiles("cpp", "cpp_");

            var prompt = Resources.Read(typeof(PlatformIOBuilder).Assembly, "Cyrena.PlatformIO.Resources.prompt.md");
            var sb = new StringBuilder();
            sb.AppendLine($"Environment: {options.ChatConfiguration[PlatformIOOptions.Environment]}");
            prompt = prompt.Replace("{ENVIRONMENT_CONTEXT}", sb.ToString());
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
            var dialogService = services.GetRequiredService<IDialogService>();
            var parameters = new DialogParameters<Configure>
            {
                { x => x.Model, config }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small };
            var dialog = await dialogService.ShowAsync<Configure>("PlatformIO", parameters, options);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
                await _kernel.UpdateAsync(config, true);
        }
    }
}
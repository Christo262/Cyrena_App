using Cyrena.Angular.Components.Shared;
using Cyrena.Angular.Extensions;
using Cyrena.Angular.Options;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using MudBlazor;

namespace Cyrena.Angular.Services
{
    internal class AngularBuilder : ICodeBuilder
    {
        private readonly IKernelController _kernel;
        public AngularBuilder(IKernelController kernel)
        {
            _kernel = kernel;
        }

        public string Id => AngularOptions.BuilderId;

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var angularJsonPath = options.ChatConfiguration[AngularOptions.AngularJson]
                ?? throw new InvalidOperationException("angular.json not configured. Use the Configure dialog to select your angular.json file.");

            var rootDir = Path.GetDirectoryName(angularJsonPath)
                ?? throw new InvalidOperationException("Invalid angular.json path.");

            var plan = new DevelopPlan(rootDir);
            plan.IndexAngularDefaultPlan();
            options.Plugins.AddFromType<AngularKernelFunctions>("ng");
            var prompt = Resources.Read(typeof(AngularBuilder).Assembly, "Cyrena.Angular.Resources.prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.KernelBuilder.AddStartupTask<ComponentFolderWatcher>();
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
            var dialog = await dialogService.ShowAsync<Configure>("Angular", parameters, options);
            var result = await dialog.Result;
            if (result is not null && !result.Canceled)
                await _kernel.UpdateAsync(config, true);
        }
    }
}
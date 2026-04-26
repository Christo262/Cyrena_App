using BootstrapBlazor.Components;
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

            // Comprehensive Angular project indexing
            plan.IndexAngularDefaultPlan();

            // Register the Angular plugin
            options.Plugins.AddFromType<Angular>();

            // Add the Angular system prompt
            var prompt = Resources.Read(typeof(AngularBuilder).Assembly, "Cyrena.Angular.Resources.prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);

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
                Title = "Angular",
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

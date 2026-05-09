using BootstrapBlazor.Components;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Website.Components.Shared;
using Cyrena.Website.Extensions;
using Cyrena.Website.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Website.Services
{
    internal class WebsiteCodeBuilder : ICodeBuilder
    {
        private readonly IKernelController _kernel;

        public WebsiteCodeBuilder(IKernelController kernel)
        {
            _kernel = kernel;
        }

        public string Id => Options.WebsiteOptions.BuilderId;

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var rootDir = options.ChatConfiguration[DevelopOptions.RootDirectory];
            var plan = new DevelopPlan(rootDir!);
            plan.IndexStaticWebsiteDefaultPlan();

            options.Plugins.AddFromType<WebsiteKernelFunctions>("Content");

            var promptManager = options.GetFeatureOption<IPromptManager>();
            promptManager.AddPrompt(0, Resources.Read(typeof(WebsiteCodeBuilder).Assembly, "Cyrena.Website.Resources.prompt.md"));
            options.AddToolbarComponent<BrowseDirectory>(ToolbarAlignment.Start);
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
            var result = await dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Website",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    { nameof(Configure.Model), config },
                    {nameof(Configure.IsEdit), true }
                },
                ButtonYesText = "Save",
                ButtonNoText = "Cancel",
            });

            if (result == DialogResult.Yes)
                await _kernel.UpdateAsync(config, true);
        }
    }
}

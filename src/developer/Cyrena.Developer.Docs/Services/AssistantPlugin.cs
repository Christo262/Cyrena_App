using Cyrena.Contracts;
using Cyrena.Developer.Docs.Components.Shared;
using Cyrena.Developer.Docs.Models;
using Cyrena.Developer.Docs.Plugins;
using Cyrena.Developer.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Options;
using Microsoft.SemanticKernel;

namespace Cyrena.Developer.Docs.Services
{
    internal class AssistantPlugin : IAssistantPlugin
    {
        public string[] Modes => [DevelopOptions.AssistantModeId];

        public int Priority => 20;

        public string Id => "cyrena.api_references";

        public bool Required => true;

        public string Title => "API References";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();    
            persistence.AddSingletonStore<ApiReference>("api_references");
            builder.Plugins.AddFromType<APIReferences>();

            builder.KernelBuilder.AddToolbarComponent<ToolbarIcon>(ToolbarAlignment.End);
            return Task.CompletedTask;
        }
    }
}

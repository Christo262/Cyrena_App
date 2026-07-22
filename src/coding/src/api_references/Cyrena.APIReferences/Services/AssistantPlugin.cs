using Cyrena.Contracts;
using Cyrena.APIReferences.Components.Shared;
using Cyrena.APIReferences.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Options;
using Microsoft.SemanticKernel;
using Cyrena.Coding.Options;

namespace Cyrena.APIReferences.Services
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
            builder.Plugins.AddFromType<APIReferencesKernelFunctions>("API_reference");

            builder.AddToolbarComponent<ToolbarIcon>(ToolbarAlignment.End);
            return Task.CompletedTask;
        }
    }
}

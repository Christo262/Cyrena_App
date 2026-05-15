using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Options;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.LTM.Services
{
    internal class MemoryAssistantPlugin : IAssistantPlugin
    {
        private readonly IMemoryService _ltm;
        private readonly ISettingsService _settings;
        public MemoryAssistantPlugin(IMemoryService ltm, ISettingsService settings)
        {
            _ltm = ltm;
            _settings = settings;
        }

        public string Id => "cyrena.ltm";
        public string[] Modes => [IAssistantMode.AssistantModeDefault];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "Long-term Memory";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton(_ltm);
            builder.Plugins.AddFromType<MemoryAssistantKernelFunctions>("ltm");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(10, Resources.Read(typeof(MemoryAssistantPlugin).Assembly, "Cyrena.LTM.Resources.prompt.md"));

            var options = _settings.Read<MemoryContextOptions>(MemoryContextOptions.Key) ?? new MemoryContextOptions();

            // Ensure MemoryContextOptions is available in DI
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IConversationHistoryTransformer, MemoryContextInjectionTask>();

            return Task.CompletedTask;
        }
    }
}

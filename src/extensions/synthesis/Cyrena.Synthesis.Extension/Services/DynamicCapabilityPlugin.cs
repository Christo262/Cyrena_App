using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Synthesis.Components.Shared;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Extensions;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Synthesis.Services
{
    internal class DynamicCapabilityPlugin : IAssistantPlugin
    {
        private readonly ICapabilityStore _caps;
        private readonly ICapabilityPermissionService _perms;
        private readonly ISettingsService _settings;
        public DynamicCapabilityPlugin(ICapabilityStore caps, ICapabilityPermissionService perms, ISettingsService settings)
        {
            _caps = caps;
            _perms = perms;
            _settings = settings;
        }

        public string Id => "cyrena.synthesis";
        public string[] Modes => [];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "Dynamic Capabilities";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            if(builder.ChatConfiguration.AssistantModeId == SynthesisOptions.AssistantId)
                return Task.CompletedTask; //CapabilityBuilderAssistant will add this

            var options = _settings.Read<SynthesisOptions>(SynthesisOptions.Key) ?? new SynthesisOptions();
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton(_caps);
            builder.Services.AddSingleton(_perms);

            builder.Services.AddSingleton<ICapabilityLogger, CapabilityLogger>();
            builder.Services.AddSingleton<IScriptEngine, ScriptEngine>();
            builder.Services.AddSingleton<IScriptValidator, ScriptValidator>();
            builder.Services.AddSingleton<ICapabilityContext, CapabilityContext>();

            builder.ConfigureDefaultAbis();

            builder.AddToolbarComponent<WorkingDirectoryToolbar>(ToolbarAlignment.Start);

            builder.Plugins.AddFromType<DynamicCapabilityConsumerFunctions>("Capabilities");
            builder.Plugins.AddFromType<DynamicCapabilityRequestFunctions>("Capability");

            if (string.IsNullOrEmpty(builder.ChatConfiguration[SynthesisOptions.WorkingDirectoryKey]))
                builder.ChatConfiguration[SynthesisOptions.WorkingDirectoryKey] = options.SandboxRootDirectory;

            builder.KernelBuilder.AddStartupTask<DynamicCapabilityPromptTask>();
            return Task.CompletedTask;
        }
    }
}

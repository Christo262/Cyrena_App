using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Synthesis.Extensions;
using Cyrena.Options;

namespace Cyrena.Synthesis.Services
{
    internal class CapabilityBuilderAssistant : IAssistantMode
    {
        private readonly ICapabilityStore _caps;
        private readonly ICapabilityPermissionService _perms;
        private readonly ISettingsService _settings;
        public CapabilityBuilderAssistant(ICapabilityStore caps, ICapabilityPermissionService perms, ISettingsService settings)
        {
            _caps = caps;
            _perms = perms;
            _settings = settings;
        }

        public string Id => SynthesisOptions.AssistantId;

        public Task ConfigureAsync(CyrenaKernelBuilder builder)
        {
            var options = _settings.Read<SynthesisOptions>(SynthesisOptions.Key) ?? new SynthesisOptions();
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton(_caps);
            builder.Services.AddSingleton(_perms);

            builder.Services.AddSingleton<ICapabilityLogger, CapabilityLogger>();
            builder.Services.AddSingleton<IScriptEngine, ScriptEngine>();
            builder.Services.AddSingleton<IScriptValidator, ScriptValidator>();
            builder.Services.AddSingleton<ICapabilityContext, CapabilityContext>();

            builder.ConfigureDefaultAbis();

            builder.Plugins.AddFromType<DynamicCapabilityConsumerFunctions>("Capabilities");
            builder.Plugins.AddFromType<DynamicCapabilityBuilderFunctions>("Builder");

            builder.GetFeatureOption<IPromptManager>().AddPrompt(0, Resources.Read(typeof(DynamicCapabilityPlugin).Assembly, "Cyrena.Synthesis.Resources.prompt.md"));

            if (string.IsNullOrEmpty(builder.ChatConfiguration[SynthesisOptions.WorkingDirectoryKey]))
                builder.ChatConfiguration[SynthesisOptions.WorkingDirectoryKey] = Path.Combine(CyrenaBuilder.AppDataDirectory, ".sandbox");
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
        }

        public Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            return Task.CompletedTask;
        }
    }
}

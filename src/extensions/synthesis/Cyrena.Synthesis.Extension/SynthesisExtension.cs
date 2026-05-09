using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Persistence.Options;
using Cyrena.Synthesis.Components.Shared;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Synthesis
{
    public class SynthesisExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();
            persistence.AddSingletonStore<DynamicCapability>("synthetic_capabilities");
            persistence.AddSingletonStore<CapabilityPermission>("synthetic_capability_permissions");

            builder.Services.AddSingleton<ICapabilityStore, CapabilityStore>();
            builder.Services.AddSingleton<ICapabilityPermissionService, CapabilityPermissionService>();
            builder.AddAssistantPlugin<DynamicCapabilityPlugin>();
            builder.AddAssistantMode<CapabilityBuilderAssistant>();
            builder.AddFeatureAssembly<SynthesisExtension>("blazor");
            builder.AddSettingsComponent<Cyrena.Synthesis.Components.Shared.Index>("Dynamic Capabilities");
        }
    }
}

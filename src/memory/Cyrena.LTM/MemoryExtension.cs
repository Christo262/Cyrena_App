using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.LTM.Components.Shared;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Models;
using Cyrena.LTM.Services;
using Cyrena.Options;
using Cyrena.Persistence.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.LTM
{
    public class MemoryExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();
            persistence.AddSingletonStore<Category>("ltm-categories");
            persistence.AddSingletonStore<Entry>("ltm-entries");

            builder.Services.AddSingleton<IMemoryService, MemoryService>();
            builder.AddAssistantPlugin<MemoryAssistantPlugin>();

            builder.AddSettingsComponent<Settings>("Long-Term Memory", 10);
        }
    }
}

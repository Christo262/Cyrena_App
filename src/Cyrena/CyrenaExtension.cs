using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Services;

namespace Cyrena
{
    public class CyrenaExtension : Extension
    {
        public const string Id = "cyrena";
        public const string Name = "Cyréna";
        public const string Description = "Cyréna core application runtime.";
        public static Version Version = System.Version.Parse("0.4.0");

        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddComponents();
            builder.AddExtensaComponents();
            builder.AddOllama();
            builder.AddOpenAI();

            builder.AddAssistantMode<DefaultAssistantMode>();
            builder.AddAssistantPlugin<DefaultAssistantPlugin>();
        }
    }
}

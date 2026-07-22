using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Ollama.Web.Components.Shared;
using Cyrena.Ollama.Web.Contracts;
using Cyrena.Ollama.Web.Services;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Ollama.Web
{
    public class OllamaWebExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddSettingsComponent<Settings>("Ollama", 20);
            builder.AddAssistantPlugin<OllamaWebAssistantsPlugin>();
        }
    }
}

using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Ollama.Web.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Ollama.Web.Services
{
    internal class OllamaWebAssistantsPlugin : IAssistantPlugin
    {
        private readonly ISettingsService _web;
        public OllamaWebAssistantsPlugin(ISettingsService web)
        {
            _web = web;
        }

        public string Id => "ollama.web";
        public string[] Modes => [];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "Ollama Web Search";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IOllamaWebSearchService>(new OllamaWebSearchService(_web));
            builder.Plugins.AddFromType<OllamaWebKernelFunctions>("OllWeb");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(10, Resources.Read(typeof(OllamaWebAssistantsPlugin).Assembly, "Cyrena.Ollama.Web.Resources.prompt.md"));
            return Task.CompletedTask;
        }
    }
}

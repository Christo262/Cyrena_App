using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Tavily.Options;
using Cyrena.Tavily.Plugins;
using Cyrena.Models;

namespace Cyrena.Tavily.Services
{
    internal class TavilyExtension : IAssistantPlugin
    {
        private readonly ISettingsService _settings;
        public TavilyExtension(ISettingsService settings)
        {
            _settings = settings;
        }

        public int Priority => 10;

        public string[] Modes => [];

        public string Id => "cyrena.tavily";

        public bool Required => false;

        public string Title => "Tavily Web Search";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            var options = _settings.Read<TavilyOptions>(TavilyOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.ApiKey) || !options.Enable)
                return Task.CompletedTask;
            builder.Services.AddSingleton(options);
            builder.Plugins.AddFromType<Internet>();
            return Task.CompletedTask;
        }
    }
}

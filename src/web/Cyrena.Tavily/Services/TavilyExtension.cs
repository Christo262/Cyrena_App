using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Tavily.Options;
using Cyrena.Tavily.Plugins;

namespace Cyrena.Tavily.Services
{
    internal class TavilyExtension : IDeveloperContextExtension
    {
        private readonly ISettingsService _settings;
        public TavilyExtension(ISettingsService settings)
        {
            _settings = settings;
        }

        public int Priority => 10;

        public Task ExtendAsync(IDeveloperContextBuilder builder)
        {
            var options = _settings.Read<TavilyOptions>(TavilyOptions.Key);
            if(options == null || string.IsNullOrEmpty(options.ApiKey) || !options.Enable)
                return Task.CompletedTask;
            builder.Services.AddSingleton(options);
            builder.Plugins.AddFromType<Internet>();
            return Task.CompletedTask;
        }
    }
}

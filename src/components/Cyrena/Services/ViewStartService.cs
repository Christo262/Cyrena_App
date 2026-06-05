using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ViewStartService : IViewStart
    {
        private readonly ISettingsService _settings;
        private IEnumerable<IViewStartProvider> _providers;
        public ViewStartService(ISettingsService settings, IServiceProvider services)
        {
            _settings = settings;
            _providers = services.GetServices<IViewStartProvider>();
        }

        public ViewStart GetViewStart()
        {
            ViewStart? start = null;
            var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
            foreach (var provider in _providers)
            {
                var starts = provider.Provide();
                start = starts.FirstOrDefault(x => x.Id == customs.ViewStart);
                if (start != null)
                    break;
            }

            return start ?? new ViewStart("cyrena.default", typeof(DefaultViewStart), "Start Conversation", null);
        }
    }
}

using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Components.Shared
{
    public partial class PluginSelector
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Parameter]
        [EditorRequired]
        public ChatConfiguration Chat { get; set; }

        private IEnumerable<PluginSelect> _models = default!;
        protected override void OnInitialized()
        {
            var plugins = _services.GetServices<IAssistantPlugin>()
                .Where(x => x.Modes.Length == 0 || x.Modes.Contains(Chat.AssistantModeId));
            _models = plugins.Select(x => new PluginSelect(x)).ToList();
            if(!Chat.PluginIds.Any())
                foreach (var item in _models)
                    item.Selected = true;
            else
            {
                foreach (var item in _models)
                    item.Selected = item.Required|| Chat.PluginIds.Any(x => x == item.Plugin.Id);
            }
        }

        private void PopulateChat()
        {
            var ids = _models.Where(x => x.Selected || x.Required).Select(x => x.Id);
            Chat.PluginIds = ids.ToList();
        }
    }

    internal record PluginSelect(IAssistantPlugin Plugin)
    {
        public bool Selected { get; set; }
        public bool Required => Plugin.Required;
        public string Id => Plugin.Id;
    }
}

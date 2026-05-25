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
        public ChatConfiguration Chat { get; set; } = default!;

        private List<PluginSelect> _models = new();

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
                    item.Selected = item.Required || Chat.PluginIds.Any(x => x == item.Plugin.Id);
            }
        }

        private void OnSelectionChanged(PluginSelect item, bool value)
        {
            item.Selected = value;
            PopulateChat();
        }

        private void PopulateChat()
        {
            var ids = _models.Where(x => x.Selected || x.Required).Select(x => x.Id);
            Chat.PluginIds = ids.ToList();
        }
    }

    internal class PluginSelect
    {
        public PluginSelect(IAssistantPlugin plugin)
        {
            Plugin = plugin;
        }

        public IAssistantPlugin Plugin { get; }
        public bool Selected { get; set; }
        public bool Required => Plugin.Required;
        public string Id => Plugin.Id;
    }
}

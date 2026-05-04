using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Pages
{
    public partial class SetupComplete
    {
        [CascadingParameter]
        public TabItem? Item { get; set; }
        [CascadingParameter]
        public Tab? Parent { get; set; }

        [Inject] private NavigationManager _nav { get; set; } = default!;

        private async Task NavigateCloseTab(string url)
        {
            _nav.NavigateTo(url);
            await Task.Delay(100);
            if (Item != null && Parent != null)
                await Parent.RemoveTab(Item);
        }
    }
}

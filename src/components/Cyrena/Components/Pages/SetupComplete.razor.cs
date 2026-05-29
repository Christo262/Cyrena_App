using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Pages
{
    public partial class SetupComplete
    {
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private async Task NavigateCloseTab(string url)
        {
            _nav.NavigateTo(url);
            await Task.Delay(100);
        }
    }
}

using Cyrena.Attributes;
using Cyrena.Contracts;

namespace Cyrena.Components.Shared
{
    public partial class Docking
    {
        private List<IDockingService.DockRequest> _requests = new();
        private bool _show { get; set; }
        [KernelInject] private IDockingService _dock { get; set; } = default!;

        protected override void OnInitialized()
        {
            _dock.OnDockRequest(e =>
            {
                _requests.Add(e);
                _show = true;
                this.InvokeAsync(StateHasChanged);
            });
        }

        private Task CloseAll()
        {
            foreach(var item in _requests)
                item.OnClose();
            _show = false;
            _requests.Clear();
            this.StateHasChanged();
            return Task.CompletedTask;
        }
    }
}

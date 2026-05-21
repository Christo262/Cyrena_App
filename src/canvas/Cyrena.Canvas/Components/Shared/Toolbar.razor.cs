using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Contracts;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class Toolbar : IDisposable
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        [KernelInject] private IDockingService _dock { get; set; } = default!;
        private List<IDisposable> _disposables { get; set; } = new List<IDisposable>();

        protected override void OnInitialized()
        {
            _disposables.Add(_canvas.OnDocumentActivate(doc =>
            {
                this.InvokeAsync(async () =>
                {
                    Toggle();
                });
            }));
        }

        private bool _open { get; set; }
        private void Toggle()
        {
            if (_open) return;
            _dock.Dock<CanvasPreview>("Preview", () =>
            {
                _open = false;
                this.InvokeAsync(StateHasChanged);
            });
            _dock.Dock<CanvasEditor>("Code", () => { });
            _open = true;
        }

        public void Dispose()
        {
            foreach(var dsp in _disposables)
                dsp.Dispose();
        }
    }
}

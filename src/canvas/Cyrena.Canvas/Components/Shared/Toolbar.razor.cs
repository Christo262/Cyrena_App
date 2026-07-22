using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Contracts;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class Toolbar : IDisposable
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        [KernelInject] private IDockingService _dock { get; set; } = default!;
        private List<IDisposable> _disposables { get; set; } = new List<IDisposable>();
        private CanvasDocument? _current { get; set; }
        protected override void OnInitialized()
        {
            _disposables.Add(_canvas.OnDocumentActivate(doc =>
            {
                _current = doc;
                this.InvokeAsync( Toggle);
            }));
        }

        private bool _open { get; set; }
        private void Toggle()
        {
            if (_open) return;
            if(_current?.DocumentType == CanvasDocumentType.Html || _current?.DocumentType == CanvasDocumentType.Markdown)
            {
                _dock.Dock<CanvasPreview>("Preview", () => { });
                _dock.Dock<CanvasEditor>("Code", () => {
                    _open = false;
                    this.InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                _dock.Dock<CanvasEditor>("Code", () => {
                    _open = false;
                    this.InvokeAsync(StateHasChanged);
                });
                _dock.Dock<CanvasPreview>("Preview", () => { });
            }
            _open = true;
        }

        public void Dispose()
        {
            foreach(var dsp in _disposables)
                dsp.Dispose();
        }
    }
}

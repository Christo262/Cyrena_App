using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Components.Shared;
using Cyrena.Canvas.Models;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class CanvasEditor
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        private List<IDisposable> _disposables { get; set; } = new List<IDisposable>();
        private CanvasDocument? _current { get; set; }
        private CodeInput? _code { get; set; }

        protected override void OnInitialized()
        {
            _disposables.Add(_canvas.OnDocumentActivate(doc =>
            {
                _current = doc;
                this.InvokeAsync(async () =>
                {
                    if (_code != null)
                        await _code.UpdateValue(_current.Content);
                    StateHasChanged();
                });
            }));

            _disposables.Add(_canvas.OnDocumentUpdate(doc =>
            {
                if (_current == null)
                    _current = doc;
                _current.Content = doc.Content;
                this.InvokeAsync(async () =>
                {
                    if (_code != null)
                        await _code.UpdateValue(_current.Content);
                    StateHasChanged();
                });
            }));
            _disposables.Add(_canvas.OnDocumentDelete(doc =>
            {
                _current = null;
                this.InvokeAsync(async () =>
                {
                    StateHasChanged();
                });
            }));
            _current = _canvas.Current;
        }

        private async Task SaveAsync()
        {
            if (_current == null) return;
            await _canvas.SaveAsync(_current);
        }

        private string _lang
        {
            get
            {
                if (_current == null) return "plaintext";
                switch (_current.DocumentType)
                {
                    case CanvasDocumentType.Html:
                        return "html";
                    case CanvasDocumentType.Markdown:
                        return "markdown";
                    default:
                        return "plaintext";
                }
            }
        }
    }
}

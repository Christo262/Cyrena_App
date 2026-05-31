using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Canvas.Options;
using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class CanvasEditor
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        [KernelInject] private ICyrenaFileExporter _exporter { get; set; } = default!;
        [KernelInject] private IIterationService _its { get; set; } = default!;
        [Inject] private IFileDialog _dialog { get; set; } = default!;
        [Inject] private ISnackbar _toasts { get; set; } = default!;
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

        private IEnumerable<CanvasDocument> _docs = Enumerable.Empty<CanvasDocument>();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _docs = await _canvas.ListAsync();
            this.StateHasChanged();
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
                        return _current.Language ?? "plaintext";
                }
            }
        }

        private async Task ChangeActive(string? e)
        {
            if(!_its.Inferring && !string.IsNullOrEmpty(e))
                await _canvas.ActivateAsync(e);
        }

        private async Task ExportAsync()
        {
            if (_current?.DocumentType != CanvasDocumentType.Html && _current?.DocumentType != CanvasDocumentType.Markdown)
                return;
            var path = await _dialog.ShowSaveFileAsync("Export Canvas", (".cyrena", [".cyrena"]));
            if (string.IsNullOrEmpty(path))
                return;
            try
            {
                var properties = new Dictionary<string, string?>();
                properties[CanvasOptions.Entry] = _current.Id;
                var manifest = await _exporter.ExportFilesAsync(CanvasOptions.ExtensionId, CanvasOptions.Version, CanvasOptions.ImporterId, properties, path);
                _toasts.Add("Export complete", Severity.Success);
                var dir = Path.GetDirectoryName(path);
                if (dir != null)
                    _dialog.ExploreFolder(dir);
            }
            catch (Exception ex)
            {
                _toasts.Add(ex.Message, Severity.Error);
            }
        }

        private async Task SaveFileAsync()
        {
            if (_current == null) return;
            var ext = Path.GetExtension(_current.Name);
            if (string.IsNullOrEmpty(ext)) return;
            var path = await _dialog.ShowSaveFileAsync("Save File", (ext, [ext]));
            if (!string.IsNullOrEmpty(path))
                File.WriteAllText(path, _current.Content);
            var dir = Path.GetDirectoryName(path);
            if (dir != null)
                _dialog.ExploreFolder(dir);
        }
    }
}

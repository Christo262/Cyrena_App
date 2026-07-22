using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Markdig;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class CanvasPreview : IDisposable
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        private List<IDisposable> _disposables { get; set; } = new List<IDisposable>();
        private CanvasDocument? _current { get; set; }
        private Markdig.MarkdownPipeline _mdp = default!;
        private string? _previewHtml { get; set; }

        protected override void OnInitialized()
        {
            _mdp = new Markdig.MarkdownPipelineBuilder()
                 .UseAdvancedExtensions()
                 .Build();
            _disposables.Add(_canvas.OnDocumentActivate(doc =>
            {
                _current = doc;
                this.InvokeAsync(async () =>
                {
                    UpdatePreview();
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
                    UpdatePreview();
                    StateHasChanged();
                });
            }));
            _disposables.Add(_canvas.OnDocumentDelete(doc =>
            {
                _current = null;
                this.InvokeAsync(async () =>
                {
                    UpdatePreview();
                    StateHasChanged();
                });
            }));
            _current = _canvas.Current;
            UpdatePreview();
        }

        private IEnumerable<CanvasDocument> _items = Enumerable.Empty<CanvasDocument>();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _items = await _canvas.ListAsync();
            this.StateHasChanged();
        }

        private async Task Activate(CanvasDocument item)
        {
            await _canvas.ActivateAsync(item.Id);
        }

        public void Dispose()
        {
            foreach (var dsp in _disposables)
                dsp.Dispose();
        }

        private void UpdatePreview()
        {
            if (_current == null || string.IsNullOrEmpty(_current.Content))
            {
                _previewHtml = null;
                return;
            }
            switch (_current.DocumentType)
            {
                case CanvasDocumentType.Markdown:
                    _previewHtml = Markdig.Markdown.ToHtml(_current.Content, _mdp);
                    break;
                default:
                    _previewHtml = _current.Content;
                    break;
            }
        }
    }
}

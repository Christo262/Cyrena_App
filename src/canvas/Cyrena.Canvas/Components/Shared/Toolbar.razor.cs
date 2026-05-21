using BootstrapBlazor.Components;
using Cyrena.Attributes;
using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Markdig;
using Microsoft.AspNetCore.Components;
using System.Web;

namespace Cyrena.Canvas.Components.Shared
{
    public partial class Toolbar : IDisposable
    {
        [KernelInject] private ICanvasService _canvas { get; set; } = default!;
        [Inject] private IFileDialog _files { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        private List<IDisposable> _disposables { get; set; } = new List<IDisposable>();

        private CanvasDocument? _current { get; set; }
        private CodeInput? _code { get; set; }
        private Markdig.MarkdownPipeline _mdp = default!;

        private string? _previewHtml { get; set; }
        private ElementReference? _i_frame { get; set; }

        protected override void OnInitialized()
        {
            _mdp = new Markdig.MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            _disposables.Add(_canvas.OnDocumentActivate(doc =>
            {
                if(!_open)
                    _open = true;
                _current = doc;
                this.InvokeAsync(async () =>
                {
                    if (_code != null)
                        await _code.UpdateValue(_current.Content);
                    UpdatePreview();
                    StateHasChanged();
                });
            }));

            _disposables.Add(_canvas.OnDocumentUpdate(doc =>
            {
                if(_current == null)
                    _current = doc;
                _current.Content = doc.Content;
                this.InvokeAsync(async () =>
                {
                    if (_code != null)
                        await _code.UpdateValue(_current.Content);
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
        }

        private async Task SaveAsync()
        {
            if (_current == null) return;
            await _canvas.SaveAsync(_current);
            UpdatePreview();
            this.StateHasChanged();
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

        private bool _open { get; set; }
        private void Toggle()
        {
            _open = !_open;
        }

        public void Dispose()
        {
            foreach(var dsp in _disposables)
                dsp.Dispose();
        }
        private async Task Print()
        {
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

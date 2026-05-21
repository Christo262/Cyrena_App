using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.Text;

namespace Cyrena.Canvas.Services
{
    internal class CanvasService : ICanvasService
    {
        internal const string CanvasTitle = "canvas.title";
        private readonly IFileHandlerFactory _files;
        private readonly CanvasPipeline _pipeline;
        public CanvasService(IFileHandlerFactory files)
        {
            _files = files;
            _pipeline = new CanvasPipeline();
        }

        public CanvasDocument? Current { get; private set; }      
        
        public async Task<CanvasDocument> CreateAsync(string title, CanvasDocumentType type, CancellationToken cancellationToken = default)
        {
            string name;
            string content;
            string contentType;
            switch (type)
            {
                case CanvasDocumentType.Html:
                    name = $"{title}.html";
                    content = $"<body style=\"background-color:white;\"><h1>{title}</h1></body>";
                    contentType = "text/html" ;
                    break;
                case CanvasDocumentType.Markdown:
                    name = $"{title}.md";
                    content = $"# {title}";
                    contentType= "text/markdown" ;
                    break;
                default:
                    name = $"{title}.txt";
                    content = title;
                    contentType = "text/plain" ;
                    break;
            }
            var data = Encoding.UTF8.GetBytes(content);
            var att = await _files.CreateAsync(name, contentType, data, cancellationToken);
            att.Properties[CanvasTitle] = title;
            att.Tools.AddRange(["Canvas_activate", "Canvas_delete", "Canvas_write", "Canvas_get_active"]);
            await _files.UpdateAsync(att, cancellationToken);
            var doc = new CanvasDocument()
            {
                DocumentType = type,
                Title = title,
                Id = att.Id
            };
            _pipeline.InvokeDocumentCreate(doc);
            return doc;
        }

        public async Task<IEnumerable<CanvasDocument>> ListAsync(CancellationToken cancellationToken = default)
        {
            var files = await _files.ListAttachmentsAsync(cancellationToken);
            var docs = new List<CanvasDocument>();
            foreach (var file in files)
            {
                if (file.Tools.Contains("Canvas_activate"))
                    docs.Add(new CanvasDocument()
                    {
                        Title = file[CanvasTitle],
                        Id = file.Id,
                        DocumentType = file.Id.EndsWith(".html") ? CanvasDocumentType.Html : file.Id.EndsWith(".md") ? CanvasDocumentType.Markdown : CanvasDocumentType.Text
                    });
            }
            return docs;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var doc = await GetDocumentAsync(id);
            if (doc == null) return;
            _pipeline.InvokeDocumentDelete(doc);
            await _files.DeleteFileAttachmentAsync(id, cancellationToken);
        }

        public async Task<bool> ActivateAsync(string id, CancellationToken cancellationToken = default)
        {
            var doc = await GetDocumentAsync(id, cancellationToken);
            if(doc == null) return false;
            Current = doc;
            _pipeline.InvokeDocumentActivate(Current);
            return true;
        }

        public async Task<CanvasDocument> WriteAsync(string content, int startLine = 0, int lineCount = 0, CancellationToken cancellationToken = default)
        {
            if (Current == null)
                throw new NullReferenceException("No canvas document active");
            var att = await _files.GetAttachmentAsync(Current.Id, cancellationToken);
            if(att == null)
                throw new NullReferenceException("Something wrong in file system");
            var lines = Current.Content?
                .Split("\n")
                .ToList() ?? new List<string>();

            if (startLine < 0 || startLine > lines.Count)
                throw new InvalidOperationException("Invalid startIndex");

            if (lineCount < 0 || startLine + lineCount > lines.Count)
                throw new InvalidOperationException("Invalid count");

            var newLines = content
                .Split("\n")
                .ToList();

            if (lineCount > 0)
                lines.RemoveRange(startLine, lineCount);

            lines.InsertRange(startLine, newLines);

            Current.Content = string.Join("\n", lines);
            File.WriteAllText(att.Path, Current.Content);
            _pipeline.InvokeDocumentUpdate(Current);
            return Current;
        }

        public async Task SaveAsync(CanvasDocument document, CancellationToken cancellationToken = default)
        {
            var att = await _files.GetAttachmentAsync(document.Id, cancellationToken);
            if (att == null)
                throw new NullReferenceException("Something wrong in file system");
            await File.WriteAllTextAsync(att.Path, document.Content);
        }

        private async Task<CanvasDocument?> GetDocumentAsync(string id, CancellationToken cancellationToken = default)
        {
            var att = await _files.GetAttachmentAsync(id, cancellationToken);
            if (att == null || !att.Tools.Contains("Canvas_activate")) return null;
            var content = await _files.GetKernelContent(id, cancellationToken);
            if (content is not TextContent text) return null;
            var doc = new CanvasDocument()
            {
                Id = id,
                Title = att[CanvasTitle],
                DocumentType = att.Id.EndsWith(".html") ? CanvasDocumentType.Html : att.Id.EndsWith(".md") ? CanvasDocumentType.Markdown : CanvasDocumentType.Text,
                Content = text.Text,
                Path = att.Path,
            };
            return doc;
        }

        public async Task<CanvasDocument> CreateFromAttachmentAsync(string originalId, CanvasDocumentType type, string title, CancellationToken cancellationToken = default)
        {
            var att = await _files.GetAttachmentAsync(originalId, cancellationToken);
            if (att == null)
                throw new FileNotFoundException($"Unable to find attachment with id {originalId}");
            var content = await _files.GetKernelContent(originalId, cancellationToken);
            if (content is not TextContent text)
                throw new InvalidOperationException($"Attachment {originalId} is not a text-content file");
            var data = await _files.GetFileDataAsync(originalId, cancellationToken);

            var natt = await _files.CreateAsync(att.InternalName, att.MimeType, data, cancellationToken);
            natt.Properties[CanvasTitle] = att.Id;
            natt.Tools.AddRange(["Canvas_activate", "Canvas_delete", "Canvas_write", "Canvas_get_active"]);
            await _files.UpdateAsync(att, cancellationToken);
            var doc = new CanvasDocument()
            {
                DocumentType = type,
                Title = title,
                Id = natt.Id
            };
            _pipeline.InvokeDocumentCreate(doc);
            return doc;
        }

        public IDisposable OnDocumentCreate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentCreate(cb);
        public IDisposable OnDocumentDelete(Action<CanvasDocument> cb) => _pipeline.WatchDocumentDelete(cb);
        public IDisposable OnDocumentActivate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentActivate(cb);
        public IDisposable OnDocumentUpdate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentUpdate(cb);
    }

    internal class CanvasPipeline : EventPipeline
    {
        public IDisposable WatchDocumentCreate(Action<CanvasDocument> cb) => this.ConfigurePipe("doc_create", cb);
        public void InvokeDocumentCreate(CanvasDocument doc) => this.InvokePipeline("doc_create", doc);

        public IDisposable WatchDocumentDelete(Action<CanvasDocument> cb) => this.ConfigurePipe("doc_del", cb);
        public void InvokeDocumentDelete(CanvasDocument doc) => this.InvokePipeline("doc_del", doc);

        public IDisposable WatchDocumentActivate(Action<CanvasDocument> cb) => this.ConfigurePipe("doc_act", cb);
        public void InvokeDocumentActivate(CanvasDocument doc) => this.InvokePipeline("doc_act", doc);

        public IDisposable WatchDocumentUpdate(Action<CanvasDocument> cb) => this.ConfigurePipe("doc_update", cb);
        public void InvokeDocumentUpdate(CanvasDocument doc) => this.InvokePipeline("doc_update", doc);
    }
}

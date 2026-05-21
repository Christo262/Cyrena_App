using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Canvas.Services
{
    internal class CanvasService : ICanvasService
    {
        private readonly IStore<CanvasDocument> _store;
        private readonly IChatConfigurationService _config;
        private readonly CanvasPipeline _pipeline;
        public CanvasService(IStore<CanvasDocument> store, IChatConfigurationService config)
        {
            _store = store;
            _config = config;
            _pipeline = new CanvasPipeline();
        }

        public CanvasDocument? Current { get; private set; }

        public async Task<IEnumerable<CanvasDocument>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _store.FindManyAsync(x => x.ConversationId == _config.Config.Id, new OrderBy<CanvasDocument>(x => x.Title, SortDirection.Ascending), ct:cancellationToken);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var ext = await _store.FindAsync(x => x.Id == id && x.ConversationId == _config.Config.Id, ct:cancellationToken);
            if(ext != null)
            {
                await _store.DeleteAsync(ext);
                _pipeline.InvokeDocumentDelete(ext);
            }
        }

        public async Task<CanvasDocument> CreateAsync(string title, CanvasDocumentType documentType, CancellationToken cancellationToken = default)
        {
            var doc = new CanvasDocument()
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                DocumentType = documentType,
                ConversationId = _config.Config.Id
            };
            await _store.AddAsync(doc, cancellationToken);
            _pipeline.InvokeDocumentCreate(doc);
            return doc;
        }

        public async Task<bool> ActivateAsync(string id, CancellationToken cancellationToken = default)
        {
            var ext = await _store.FindAsync(x => x.Id == id && x.ConversationId == _config.Config.Id, ct: cancellationToken);
            if(ext == null)return false;
            Current = ext;
            _pipeline.InvokeDocumentActivate(ext);
            return true;
        }

        public async Task<CanvasDocument> WriteAsync(string content, int startLine = 0, int lineCount = 0, CancellationToken cancellationToken =  default)
        {
            if (Current == null)
                throw new NullReferenceException("No canvas document active");

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
            await _store.UpdateAsync(Current, cancellationToken);
            _pipeline.InvokeDocumentUpdate(Current);
            return Current;
        }

        public IDisposable OnDocumentCreate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentCreate(cb);
        public IDisposable OnDocumentDelete(Action<CanvasDocument> cb) => _pipeline.WatchDocumentDelete(cb);
        public IDisposable OnDocumentActivate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentActivate(cb);
        public IDisposable OnDocumentUpdate(Action<CanvasDocument> cb) => _pipeline.WatchDocumentUpdate(cb);

        public async Task SaveAsync(CanvasDocument document, CancellationToken cancellationToken = default)
        {
            await _store.SaveAsync(document, cancellationToken);
        }
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

using Cyrena.Canvas.Models;

namespace Cyrena.Canvas.Contracts
{
    /// <summary>
    /// Service to interact with the canvas. Kernel Locked
    /// </summary>
    public interface ICanvasService
    {
        CanvasDocument? Current { get;}

        Task<IEnumerable<CanvasDocument>> ListAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<CanvasDocument> CreateAsync(string title, CanvasDocumentType documentType, CancellationToken cancellationToken = default);
        Task<bool> ActivateAsync(string id, CancellationToken cancellationToken = default);
        Task<CanvasDocument> WriteAsync(string content, int startLine = 0, int lineCount = 0, CancellationToken cancellationToken = default);
        Task<CanvasDocument> CreateFromAttachmentAsync(string originalId, CanvasDocumentType type, string title, CancellationToken cancellationToken = default);

        IDisposable OnDocumentCreate(Action<CanvasDocument> cb);
        IDisposable OnDocumentDelete(Action<CanvasDocument> cb);
        IDisposable OnDocumentActivate(Action<CanvasDocument> cb);
        IDisposable OnDocumentUpdate(Action<CanvasDocument> cb);

        Task SaveAsync(CanvasDocument document, CancellationToken cancellationToken = default);
        Task<string?> GetAttachmentEmbedPath(string fileId, CancellationToken cancellationToken = default);
    }
}

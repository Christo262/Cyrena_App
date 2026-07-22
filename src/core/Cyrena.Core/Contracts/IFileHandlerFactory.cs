using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Provides easier access to all <see cref="IFileHandler"/> in a <see cref="Microsoft.SemanticKernel.Kernel"/> instance. Kernel Locked
    /// </summary>
    public interface IFileHandlerFactory
    {
        bool HasFileHandlers { get; }
        bool CanHandleType(string contentType, string fileName);
        string[] GetSupportedMimeTypes();
        Task<KernelContent> GetKernelContent(string fileId, CancellationToken cancellationToken = default);
        Task<KernelContent?> SaveAsync(Stream data, string contentType, string name, CancellationToken cancellationToken = default);
        Task<KernelContent?> SaveAsync(byte[] data, string contentType, string name, CancellationToken cancellationToken = default);
        Task CancelAsync(KernelContent item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets the file extension associated with a mimetype. 
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns>'.{file extension}' or NULL if the mimetype is not supported in current Kernel instance file handlers</returns>
        string? GetExtension(string mimeType);
        Task<IEnumerable<FileAttachment>> ListAttachmentsAsync(CancellationToken cancellationToken = default);
        Task<byte[]> GetFileDataAsync(string id, CancellationToken cancellationToken = default);
        Task DeleteFileAttachmentAsync(string id, CancellationToken cancellationToken = default);
        Task<FileAttachment> CreateAsync(string name, string contentType, byte[] content, CancellationToken cancellationToken = default);
        Task UpdateAsync(FileAttachment att, CancellationToken cancellationToken = default);
        Task<FileAttachment?> GetAttachmentAsync(string id, CancellationToken cancellationToken = default);
    }
}

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
        Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name);
        Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name);
        Task<KernelContent?> GetKernelContent(Stream data, string contentType, string name);
        Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name);
        /// <summary>
        /// Gets the file extension associated with a mimetype. 
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns>'.{file extension}' or NULL if the mimetype is not supported in current Kernel instance file handlers</returns>
        string? GetExtension(string mimeType);
    }
}

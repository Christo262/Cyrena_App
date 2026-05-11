using Cyrena.Models;

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
    }
}

using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Provides file handling abilities. Kernel Locked
    /// </summary>
    public interface IFileHandler
    {
        bool HandlesType(string contentType, string fileName);
        Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name);
        Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name);
        Task<KernelContent?> GetKernelContent(Stream data, string contentType, string name);
        Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name);
        string[] GetSupportedMimeTypes();
        Dictionary<string, string> GetExtensionMimeTypeMapping();
    }
}

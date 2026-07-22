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
        Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null);
        string[] GetSupportedMimeTypes();
        Dictionary<string, string> GetExtensionMimeTypeMapping();
    }
}

using Cyrena.Models;

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
        string[] GetSupportedMimeTypes();
    }
}

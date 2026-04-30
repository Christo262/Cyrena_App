using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Provides file handling abilities
    /// </summary>
    public interface IFileHandler
    {
        bool HandlesType(string contentType, string fileName);
        Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name);
        string[] GetSupportedMimeTypes();
    }
}

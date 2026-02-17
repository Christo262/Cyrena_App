using Cyrena.Developer.Models;

namespace Cyrena.Developer.Contracts
{
    /// <summary>
    /// Used to keep in memory backup of files modified by AI
    /// </summary>
    public interface IVersionControl
    {
        void Backup(DevelopFileContent? file);
        bool HasBackup(string fileId);
        DevelopFileContent? GetBackups(string fileId);
        IEnumerable<DevelopFileContent> GetBackups();
        void RemoveBackup(string fileId);
    }
}

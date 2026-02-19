using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;

namespace Cyrena.Developer.Services
{
    internal class VersionControl : IVersionControl
    {
        private readonly Dictionary<string, DevelopFileContent> _backups;
        public VersionControl()
        {
            _backups = new Dictionary<string, DevelopFileContent>();
        }

        public void Backup(DevelopFileContent? file)
        {
            if (file == null) 
                return;
            _backups[file.Id] = file;
        }

        public bool HasBackup(string fileId)
        {
            return _backups.ContainsKey(fileId);
        }

        public DevelopFileContent? GetBackups(string fileId)
        {
            if (!_backups.ContainsKey(fileId))
                return null;
            return _backups[fileId];
        }

        public IEnumerable<DevelopFileContent> GetBackups()
        {
            return _backups.Select(x => x.Value);
        }

        public void RemoveBackup(string fileId)
        {
            if (!_backups.ContainsKey(fileId))
                return;
            _backups.Remove(fileId);
        }
    }
}

using Cyrena.Models;

namespace Cyrena.Events
{
    public class FileRenamedEvent : IEvent
    {
        public FileRenamedEvent(string fullPath, string? name, WatcherChangeTypes changeType, string oldPath, string? oldName)
        {
            FullPath = fullPath;
            Name = name;
            ChangeType = changeType;
            OldPath = oldPath;
            OldName = oldName;
        }

        public string FullPath { get; set; }
        public string? Name { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
        public string OldPath { get; set; }
        public string? OldName { get; set; }
    }
}

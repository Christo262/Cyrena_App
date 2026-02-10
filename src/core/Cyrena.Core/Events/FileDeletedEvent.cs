using Cyrena.Models;

namespace Cyrena.Events
{
    public class FileDeletedEvent : IEvent
    {
        public FileDeletedEvent(string fullPath, string? name, WatcherChangeTypes changeType)
        {
            FullPath = fullPath;
            Name = name;
            ChangeType = changeType;
        }

        public string FullPath { get; set; }
        public string? Name { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
    }
}

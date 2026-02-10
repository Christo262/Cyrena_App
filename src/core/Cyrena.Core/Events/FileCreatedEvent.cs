using Cyrena.Models;

namespace Cyrena.Events
{
    public class FileCreatedEvent : IEvent
    {
        public FileCreatedEvent(string fullPath, string? name, WatcherChangeTypes changeType)
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

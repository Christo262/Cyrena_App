using System;

namespace Cyrena.Extensa.Models
{
    public class Dependency
    {
        public Dependency() { }
        public Dependency(string id, Version minVersion)
        {
            Id = id;
            MinVersion = minVersion;
        }

        public string Id { get; set; } = default!;
        public Version MinVersion { get; set; } = default!;
    }
}

using System;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("minVersion")]
        public Version MinVersion { get; set; } = default!;
    }
}

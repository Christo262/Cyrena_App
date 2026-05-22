using System.Text.Json.Serialization;

namespace Cyrena.Models
{
    /// <summary>
    /// Repesents the basic data for Cyréna to import/export custom files that may require custom extensions.
    /// </summary>
    public sealed class CyrenaFileManifest
    {
        [JsonConstructor]
        internal CyrenaFileManifest()
        {
            Properties = new Dictionary<string, string?>();
        }
        public CyrenaFileManifest(string extension, Version version, string importerId)
        {
            Extension = extension;
            Version = version;
            ImporterId = importerId;
            Properties = new Dictionary<string, string?>();
        }

        /// <summary>
        /// The extension used to handle this file
        /// </summary>
        [JsonPropertyName("extension.required")]
        public string Extension { get; set; } = default!;
        /// <summary>
        /// The version of extension used to create the file
        /// </summary>
        [JsonPropertyName("required.extension.version.min")]
        public Version Version { get; set; } = default!;
        /// <summary>
        /// The id of the <see cref="Contracts.ICyrenaFileImporter"/> that can import this file
        /// </summary>
        [JsonPropertyName("importer.id")]
        public string ImporterId { get; set; } = default!;
        /// <summary>
        /// Additional properties for <see cref="Contracts.ICyrenaFileExporter"/> to populate
        /// </summary>
        [JsonPropertyName("cyrena.properties")]
        public Dictionary<string, string?> Properties { get; set; }

        public string? this[string key]
        {
            get => Properties.TryGetValue(key, out var value) ? value : null;
            set => Properties[key] = value;
        }
    }
}

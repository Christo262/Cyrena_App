using Cyrena.Extensa.Models;

namespace Cyrena.Extensa.Loader.Models
{
    /// <summary>
    /// Represents a loaded extension with its metadata and status information.
    /// </summary>
    public class LoadedExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedExtension"/> class.
        /// </summary>
        public LoadedExtension()
        {
            Errors = new List<Exception>();
        }

        /// <summary>
        /// Gets or sets the unique identifier of the extension.
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the display name of the extension.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Gets or sets the description of the extension.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the icon path for the extension.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the version of the extension.
        /// </summary>
        public Version Version { get; set; } = default!;

        /// <summary>
        /// Gets or sets the list of errors encountered during extension loading.
        /// </summary>
        public IList<Exception> Errors { get; set; }

        /// <summary>
        /// Gets or sets the current status of the extension.
        /// </summary>
        public ExtensionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the file system path where the extension is located.
        /// </summary>
        public string Path { get; set; } = default!;

        /// <summary>
        /// Gets or sets the dependencies required by this extension.
        /// </summary>
        public Dependency[] Dependencies { get; set; } = Array.Empty<Dependency>();

        /// <summary>
        /// Gets or sets the entry assembly file name for the extension.
        /// </summary>
        public string? EntryAssembly { get; set; }

        /// <summary>
        /// Gets or sets the content root directory for the extension.
        /// </summary>
        public string? ContentRootDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the extension requires a framework builder.
        /// </summary>
        public bool RequireFrameworkBuilder { get; set; } = true;
    }

    /// <summary>
    /// Represents the status of an extension.
    /// </summary>
    public enum ExtensionStatus
    {
        /// <summary>
        /// The extension is unloaded.
        /// </summary>
        Unloaded,

        /// <summary>
        /// The extension is loaded.
        /// </summary>
        Loaded,

        /// <summary>
        /// The extension is running at runtime.
        /// </summary>
        Runtime
    }
}
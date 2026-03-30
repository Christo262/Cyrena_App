namespace Cyrena.Extensa.Loader.Models
{
    /// <summary>
    /// Represents an exception that occurs when an extension has unmet dependencies.
    /// </summary>
    public class UnmetDependencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnmetDependencyException"/> class.
        /// </summary>
        /// <param name="extensionId">The ID of the extension with unmet dependencies.</param>
        /// <param name="extensionVersion">The version of the extension with unmet dependencies.</param>
        /// <param name="dependencyId">The ID of the missing dependency.</param>
        /// <param name="dependencyVersion">The required version of the missing dependency.</param>
        public UnmetDependencyException(string extensionId, Version extensionVersion, string dependencyId, Version dependencyVersion) 
            : base($"{extensionId} has unmet dependencies with {dependencyId}")
        {
            ExtensionId = extensionId;
            ExtensionVersion = extensionVersion;
            DependencyId = dependencyId;
            DependencyVersion = dependencyVersion;
        }

        /// <summary>
        /// Gets the ID of the extension with unmet dependencies.
        /// </summary>
        public string ExtensionId { get; set; }

        /// <summary>
        /// Gets the version of the extension with unmet dependencies.
        /// </summary>
        public Version ExtensionVersion { get; set; }

        /// <summary>
        /// Gets the ID of the missing dependency.
        /// </summary>
        public string DependencyId { get; set; }

        /// <summary>
        /// Gets the required version of the missing dependency.
        /// </summary>
        public Version DependencyVersion { get; set; }
    }
}
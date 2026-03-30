using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Models;

namespace Cyrena.Extensa.Loader.Contracts
{
    /// <summary>
    /// Contract for a registry that manages loaded extensions and runtime extensions.
    /// </summary>
    public interface IExtensionRegistry
    {
        /// <summary>
        /// Gets a read-only list of loaded extensions.
        /// </summary>
        IReadOnlyList<LoadedExtension> Extensions { get; }

        /// <summary>
        /// Adds a loaded extension to the registry.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        void AddExtension(LoadedExtension extension);
    }
}
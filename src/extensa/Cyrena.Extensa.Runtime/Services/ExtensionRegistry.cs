using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Loader.Models;
using System.Collections.Concurrent;

namespace Cyrena.Extensa.Loader.Services
{
    /// <summary>
    /// Thread-safe registry for managing loaded extensions and runtime extensions.
    /// </summary>
    internal class ExtensionRegistry : IExtensionRegistry
    {
        private readonly ConcurrentBag<LoadedExtension> _extensions;
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionRegistry"/> class.
        /// </summary>
        public ExtensionRegistry()
        {
            _extensions = new ConcurrentBag<LoadedExtension>();
        }

        /// <summary>
        /// Gets a read-only list of loaded extensions.
        /// </summary>
        public IReadOnlyList<LoadedExtension> Extensions => _extensions.ToList();

        /// <summary>
        /// Adds a loaded extension to the registry.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(LoadedExtension extension)
        {
            _extensions.Add(extension);
        }
    }
}
using Cyrena.Models;
using System;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Represents a permission grant for a specific dynamic capability to access a protected capability.
    /// Inherits from Entity for persistence via IStore<CapabilityPermission>.
    /// </summary>
    public class CapabilityPermission : Entity
    {
        /// <summary>
        /// The ID of the dynamic capability this permission applies to.
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the permission (e.g., "FileSystem.Read", "FileSystem.Write", "Network.Access").
        /// </summary>
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this permission allows.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this permission is currently granted.
        /// </summary>
        public bool IsGranted { get; set; } = false;

        /// <summary>
        /// When the permission was granted.
        /// </summary>
        public DateTime? GrantedAt { get; set; }

        /// <summary>
        /// When the permission grant expires, if ever.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// The scope of the permission (e.g., a specific directory path for file operations).
        /// </summary>
        public string? Scope { get; set; }
    }

    public record CapabiliyPermissionDescriptor(string Permission, string Description);
}

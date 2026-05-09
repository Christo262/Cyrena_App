using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Manages permissions for dynamic capabilities to access protected capabilities.
    /// </summary>
    public interface ICapabilityPermissionService
    {
        /// <summary>
        /// Grants a permission to a dynamic capability.
        /// </summary>
        Task<CapabilityPermission> GrantPermissionAsync(string scriptId, string permissionName, string? scope = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a permission from a dynamic capability.
        /// </summary>
        Task<bool> RevokePermissionAsync(string scriptId, string permissionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a dynamic capability has been granted a specific permission.
        /// </summary>
        Task<bool> HasPermissionAsync(string scriptId, string permissionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all granted (active, non-expired) permissions for a specific dynamic capability.
        /// </summary>
        Task<IReadOnlyList<CapabilityPermission>> GetGrantedPermissionsAsync(string scriptId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all permissions for a dynamic capability. Returns the number of permissions deleted.
        /// </summary>
        Task<int> DeleteAllPermissionsAsync(string scriptId, CancellationToken cancellationToken = default);
    }
}

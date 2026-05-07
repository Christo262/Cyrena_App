using Cyrena.Persistence.Contracts;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Extensions;

namespace Cyrena.Synthesis.Services
{
    /// <summary>
    /// Manages permissions for dynamic capabilities to access protected capabilities.
    /// Uses IStore<CapabilityPermission> for persistence.
    /// </summary>
    internal class CapabilityPermissionService : ICapabilityPermissionService
    {
        private readonly IStore<CapabilityPermission> _permissionStore;

        public CapabilityPermissionService(IStore<CapabilityPermission> permissionStore)
        {
            _permissionStore = permissionStore;
        }

        public async Task<CapabilityPermission> GrantPermissionAsync(string scriptId, string permissionName, string? scope = null, CancellationToken cancellationToken = default)
        {
            var existing = await _permissionStore.FindAsync(
                p => p.ScriptId == scriptId && p.PermissionName == permissionName,
                cancellationToken);

            if (existing != null)
            {
                existing.IsGranted = true;
                existing.GrantedAt = DateTime.UtcNow;
                existing.Scope = scope;
                await _permissionStore.UpdateAsync(existing, cancellationToken);
                return existing;
            }

            var newPermission = new CapabilityPermission
            {
                ScriptId = scriptId,
                PermissionName = permissionName,
                Description = GetDefaultDescription(permissionName),
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                Scope = scope
            };

            await _permissionStore.AddAsync(newPermission, cancellationToken);
            return newPermission;
        }

        public async Task<bool> RevokePermissionAsync(string scriptId, string permissionName, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionStore.FindAsync(
                p => p.ScriptId == scriptId && p.PermissionName == permissionName,
                cancellationToken);

            if (permission == null)
            {
                return false;
            }

            permission.IsGranted = false;
            await _permissionStore.UpdateAsync(permission, cancellationToken);
            return true;
        }

        public async Task<bool> HasPermissionAsync(string scriptId, string permissionName, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionStore.FindAsync(
                p => p.ScriptId == scriptId
                && p.PermissionName == permissionName
                && p.IsGranted
                && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow),
                cancellationToken);

            return permission != null;
        }

        public async Task<IReadOnlyList<CapabilityPermission>> GetGrantedPermissionsAsync(string scriptId, CancellationToken cancellationToken = default)
        {
            var results = await _permissionStore.FindManyAsync(
                p => p.ScriptId == scriptId
                && p.IsGranted
                && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow),
                ct: cancellationToken);
            return results.ToList();
        }
        public async Task<int> DeleteAllPermissionsAsync(string scriptId, CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionStore.FindManyAsync(
                p => p.ScriptId == scriptId,
                ct: cancellationToken);

            var count = permissions.Count();
            if (count > 0)
            {
                await _permissionStore.DeleteManyAsync(
                    p => p.ScriptId == scriptId,
                    cancellationToken);
            }

            return count;
        }

        private static string GetDefaultDescription(string permissionName)
        {
            return permissionName switch
            {
                "FileSystem.Read" => "Allows reading files from the sandboxed file system.",
                "FileSystem.Write" => "Allows writing files to the sandboxed file system.",
                "FileSystem.Delete" => "Allows deleting files from the sandboxed file system.",
                "Directory.Create" => "Allows creating directories in the sandboxed file system.",
                "Directory.Delete" => "Allows deleting directories from the sandboxed file system.",
                "Network.Access" => "Allows making network requests.",
                "Process.Execute" => "Allows executing external processes.",
                _ => $"Permission to access: {permissionName}"
            };
        }
    }
}

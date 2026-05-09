namespace Cyrena.Synthesis.Services
{
    /// <summary>
    /// Shared path safety utilities for sandboxed file system operations.
    /// Prevents path traversal attacks by ensuring resolved paths stay within a root directory.
    /// </summary>
    internal static class PathSafety
    {
        /// <summary>
        /// Resolves a relative path to an absolute path within the specified root directory.
        /// Throws if the resolved path escapes the root (path traversal attack).
        /// </summary>
        public static string ResolveSafePath(string rootDirectory, string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(rootDirectory, relativePath));

            if (!fullPath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Path traversal detected: '{relativePath}' attempts to escape the sandbox root '{rootDirectory}'.");
            }

            return fullPath;
        }

        /// <summary>
        /// Ensures the parent directory exists for the given path, creating it if necessary.
        /// </summary>
        public static void EnsureParentDirectory(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

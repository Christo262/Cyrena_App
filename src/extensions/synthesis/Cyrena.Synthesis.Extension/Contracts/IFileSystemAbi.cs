namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Sandboxed file system API exposed to F# scripts.
    /// All operations are restricted to approved directories.
    /// Scripts must use this API instead of System.IO directly.
    /// 
    /// Synchronous-first design: scripts should prefer ReadText and WriteText.
    /// Async methods are available for internal use but should not be used in scripts.
    /// </summary>
    public interface IFileSystemAbi
    {
        /// <summary>
        /// Reads all text from a file within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        string ReadText(string relativePath);

        /// <summary>
        /// Writes text to a file within the approved scope.
        /// Requires 'FileSystem.Write' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        void WriteText(string relativePath, string content);

        /// <summary>
        /// Checks if a file exists within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        bool Exists(string relativePath);

        /// <summary>
        /// Deletes a file within the approved scope.
        /// Requires 'FileSystem.Delete' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        void Delete(string relativePath);

        /// <summary>
        /// Lists files in a directory within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        string[] ListFiles(string relativePath = "", string searchPattern = "*");

        /// <summary>
        /// Lists directories within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        string[] ListDirectories(string relativePath = "");

        /// <summary>
        /// Creates a directory within the approved scope.
        /// Requires 'Directory.Create' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        void CreateDirectory(string relativePath);

        /// <summary>
        /// Deletes a directory within the approved scope.
        /// Requires 'Directory.Delete' permission.
        /// This is the synchronous method scripts should prefer.
        /// </summary>
        void DeleteDirectory(string relativePath, bool recursive = false);

        /// <summary>
        /// Reads all text from a file within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// Async variant for internal use. Scripts should use ReadText instead.
        /// </summary>
        Task<string> ReadTextAsync(string relativePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes text to a file within the approved scope.
        /// Requires 'FileSystem.Write' permission.
        /// Async variant for internal use. Scripts should use WriteText instead.
        /// </summary>
        Task WriteTextAsync(string relativePath, string content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// Async variant for internal use. Scripts should use Exists instead.
        /// </summary>
        Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file within the approved scope.
        /// Requires 'FileSystem.Delete' permission.
        /// Async variant for internal use. Scripts should use Delete instead.
        /// </summary>
        Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists files in a directory within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// Async variant for internal use. Scripts should use ListFiles instead.
        /// </summary>
        Task<string[]> ListFilesAsync(string relativePath = "", string searchPattern = "*", CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists directories within the approved scope.
        /// Requires 'FileSystem.Read' permission.
        /// Async variant for internal use. Scripts should use ListDirectories instead.
        /// </summary>
        Task<string[]> ListDirectoriesAsync(string relativePath = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a directory within the approved scope.
        /// Requires 'Directory.Create' permission.
        /// Async variant for internal use. Scripts should use CreateDirectory instead.
        /// </summary>
        Task CreateDirectoryAsync(string relativePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a directory within the approved scope.
        /// Requires 'Directory.Delete' permission.
        /// Async variant for internal use. Scripts should use DeleteDirectory instead.
        /// </summary>
        Task DeleteDirectoryAsync(string relativePath, bool recursive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the root directory that all operations are scoped to.
        /// </summary>
        string ScopeRoot { get; }
    }
}

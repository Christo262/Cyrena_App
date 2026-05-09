using Cyrena.Contracts;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Options;

namespace Cyrena.Synthesis.Services
{
    internal class FileSystemAbi : IFileSystemAbi
    {
        private readonly ICapabilityContext _cap_ctx;
        private readonly ICapabilityLogger _log;
        private readonly IChatConfigurationService _config;
        private readonly SynthesisOptions _options;

        private string _rootDirectory
        {
            get
            {
                return _config.Config[SynthesisOptions.WorkingDirectoryKey] ?? _options.SandboxRootDirectory;
            }
        }
        public string ScopeRoot
        {
            get
            {
                return _config.Config[SynthesisOptions.WorkingDirectoryKey] ?? _options.SandboxRootDirectory;
            }
        }

        public FileSystemAbi(IChatConfigurationService config, SynthesisOptions options, ICapabilityContext cap_ctx, ICapabilityLogger log)
        {
            _config = config;
            _options = options;
            _cap_ctx = cap_ctx;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        // ─── Synchronous Dynamic Capability-Facing API ───

        public string ReadText(string relativePath)
        {
            RequirePermission("FileSystem.Read", "read files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Reading file: {safePath}");
            return File.ReadAllText(safePath);
        }

        public void WriteText(string relativePath, string content)
        {
            RequirePermission("FileSystem.Write", "write files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            PathSafety.EnsureParentDirectory(safePath);
            _log.Debug($"Writing file: {safePath}");
            File.WriteAllText(safePath, content);
        }

        public bool Exists(string relativePath)
        {
            RequirePermission("FileSystem.Read", "check file existence");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            return File.Exists(safePath);
        }

        public void Delete(string relativePath)
        {
            RequirePermission("FileSystem.Delete", "delete files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Deleting file: {safePath}");
            File.Delete(safePath);
        }

        public string[] ListFiles(string relativePath = "", string searchPattern = "*")
        {
            RequirePermission("FileSystem.Read", "list files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            if (!Directory.Exists(safePath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(safePath, searchPattern)
                .Select(f => Path.GetRelativePath(_rootDirectory, f))
                .ToArray();
        }

        public string[] ListDirectories(string relativePath = "")
        {
            RequirePermission("FileSystem.Read", "list directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            if (!Directory.Exists(safePath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetDirectories(safePath)
                .Select(d => Path.GetRelativePath(_rootDirectory, d))
                .ToArray();
        }

        public void CreateDirectory(string relativePath)
        {
            RequirePermission("Directory.Create", "create directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Creating directory: {safePath}");
            Directory.CreateDirectory(safePath);
        }

        public void DeleteDirectory(string relativePath, bool recursive = false)
        {
            RequirePermission("Directory.Delete", "delete directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Deleting directory: {safePath} (recursive={recursive})");
            Directory.Delete(safePath, recursive);
        }

        // ─── Async API (Preserved for Internal Use) ───

        public async Task<string> ReadTextAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Read", "read files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Reading file: {safePath}");
            return await File.ReadAllTextAsync(safePath, cancellationToken);
        }

        public async Task WriteTextAsync(string relativePath, string content, CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Write", "write files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            PathSafety.EnsureParentDirectory(safePath);
            _log.Debug($"Writing file: {safePath}");
            await File.WriteAllTextAsync(safePath, content, cancellationToken);
        }

        public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Read", "check file existence");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            return Task.FromResult(File.Exists(safePath));
        }

        public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Delete", "delete files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Deleting file: {safePath}");
            File.Delete(safePath);
            return Task.CompletedTask;
        }

        public Task<string[]> ListFilesAsync(string relativePath = "", string searchPattern = "*", CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Read", "list files");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            if (!Directory.Exists(safePath))
            {
                return Task.FromResult(Array.Empty<string>());
            }

            var files = Directory.GetFiles(safePath, searchPattern)
                .Select(f => Path.GetRelativePath(_rootDirectory, f))
                .ToArray();

            return Task.FromResult(files);
        }

        public Task<string[]> ListDirectoriesAsync(string relativePath = "", CancellationToken cancellationToken = default)
        {
            RequirePermission("FileSystem.Read", "list directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            if (!Directory.Exists(safePath))
            {
                return Task.FromResult(Array.Empty<string>());
            }

            var dirs = Directory.GetDirectories(safePath)
                .Select(d => Path.GetRelativePath(_rootDirectory, d))
                .ToArray();

            return Task.FromResult(dirs);
        }

        public Task CreateDirectoryAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            RequirePermission("Directory.Create", "create directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Creating directory: {safePath}");
            Directory.CreateDirectory(safePath);
            return Task.CompletedTask;
        }

        public Task DeleteDirectoryAsync(string relativePath, bool recursive = false, CancellationToken cancellationToken = default)
        {
            RequirePermission("Directory.Delete", "delete directories");
            var safePath = PathSafety.ResolveSafePath(_rootDirectory, relativePath);
            _log.Debug($"Deleting directory: {safePath} (recursive={recursive})");
            Directory.Delete(safePath, recursive);
            return Task.CompletedTask;
        }

        private void RequirePermission(string permission, string operation)
        {
            if(!string.IsNullOrEmpty(_rootDirectory) && !Directory.Exists(_rootDirectory))
                Directory.CreateDirectory(_rootDirectory);
            if(!_cap_ctx.RequestPermissionAsync(_cap_ctx.Current!, new Models.CapabiliyPermissionDescriptor(permission, operation)).GetAwaiter().GetResult())
                throw new UnauthorizedAccessException(
                    $"Dynamic capability does not have the required permission '{permission}' to {operation}.");
        }
    }
}

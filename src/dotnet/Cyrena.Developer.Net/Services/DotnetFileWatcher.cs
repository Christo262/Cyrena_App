using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Extensions;

namespace Cyrena.Developer.Services
{
    internal class DotnetFileWatcher : IDisposable
    {
        private readonly SolutionViewModel _sln;
        private readonly FileSystemWatcher _watcher;
        private readonly IChatMessageService _chat;
        private readonly IEnumerable<IDotnetProjectType> _projs;

        private static readonly HashSet<string> ImportantFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Attributes",
            "Contracts",
            "Extensions",
            "Components",
            "Models",
            "Services",
            "Options",
            Path.Combine("Components", "Pages"),
            Path.Combine("Components", "Layout"),
            Path.Combine("Components", "Shared")
        };

        private static readonly HashSet<string> ImportantExtensions = new HashSet<string>
        {
            // C# and .NET
            ".cs", ".csproj", ".sln", ".vb", ".vbproj", ".fs", ".fsproj",
            
            // Web files
            ".html", ".htm", ".css", ".js", ".jsx", ".ts", ".tsx", ".json",
            ".razor", ".cshtml", ".vbhtml",
            
            // Configuration
            ".config", ".xml", ".yaml", ".yml", ".settings",
            
            // Resources
            ".resx", ".xaml",
            
            // SQL and data
            ".sql",
        };

        public DotnetFileWatcher(SolutionViewModel sln, IChatMessageService chat, IEnumerable<IDotnetProjectType> projs)
        {
            _sln = sln;
            _chat = chat;
            _projs = projs;
            _watcher = new FileSystemWatcher(_sln.RootDirectory)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            _watcher.Error += _watcher_Error;
            _watcher.Created += _watcher_Created;
            _watcher.Deleted += _watcher_Deleted;
            _watcher.Renamed += _watcher_Renamed;
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (IsFileOfInterest(e.FullPath) || IsFileOfInterest(e.OldFullPath))
            {
                var fileName = Path.GetFileName(e.OldFullPath);
                foreach (var item in _sln.Projects.Where(x => x.Plan != null))
                {
                    if (item.Plan!.TryFindFileByName(fileName, out var file))
                    {
                        var proj = _projs.FirstOrDefault(x => x.Id == item.ProjectTypeId);
                        if (proj != null)
                            item.Plan = proj.IndexPlan(item);
                    }
                }
            }
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (IsFileOfInterest(e.FullPath))
            {
                var fileName = Path.GetFileName(e.FullPath);
                foreach (var item in _sln.Projects.Where(x => x.Plan != null))
                {
                    if (item.Plan!.TryFindFileByName(fileName, out var file))
                    {
                        var proj = _projs.FirstOrDefault(x => x.Id == item.ProjectTypeId);
                        if (proj != null)
                            item.Plan = proj.IndexPlan(item);
                    }
                }
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (IsFileOfInterest(e.FullPath))
            {
                var fileName = Path.GetFileName(e.FullPath);
                foreach (var item in _sln.Projects.Where(x => x.Plan != null))
                {
                    var proj = _projs.FirstOrDefault(x => x.Id == item.ProjectTypeId);
                    if (item.Plan!.TryFindFileByName(fileName, out var file))
                    {                 
                        if (proj != null)
                            item.Plan = proj.IndexPlan(item);
                    }
                    else
                    {
                        if (proj != null)
                            item.Plan = proj.IndexPlan(item);
                    }
                }
            }
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _chat.LogError(e.GetException().Message);
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Error -= _watcher_Error;
            _watcher.Created -= _watcher_Created;
            _watcher.Deleted -= _watcher_Deleted;
            _watcher.Renamed -= _watcher_Renamed;
            _watcher.Dispose();
        }

        private bool IsDirectory(string path)
        {
            return Directory.Exists(path) || (!File.Exists(path) && string.IsNullOrEmpty(Path.GetExtension(path)));
        }

        private bool IsFolderOfInterest(string fullPath)
        {
            var relativePath = Path.GetRelativePath(_sln.RootDirectory, fullPath);

            // Check if any part of the path matches our important folders
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Check for exact folder match or nested important folders
            foreach (var importantFolder in ImportantFolders)
            {
                var importantParts = importantFolder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Check if the path ends with this important folder structure
                if (pathParts.Length >= importantParts.Length)
                {
                    bool matches = true;
                    for (int i = 0; i < importantParts.Length; i++)
                    {
                        if (!pathParts[pathParts.Length - importantParts.Length + i].Equals(importantParts[i], StringComparison.OrdinalIgnoreCase))
                        {
                            matches = false;
                            break;
                        }
                    }
                    if (matches) return true;
                }
            }

            return false;
        }

        private bool IsFileOfInterest(string fullPath)
        {
            // Ignore build output directories
            var dirName = Path.GetDirectoryName(fullPath);
            if (dirName != null)
            {
                var parts = dirName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (parts.Any(p => p.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals(".vs", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals(".vscode", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals(".cyrena", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals("node_modules", StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            // Check for important file extensions
            var ext = Path.GetExtension(fullPath)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) return false;

            return ImportantExtensions.Contains(ext);
        }
    }
}
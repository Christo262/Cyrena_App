using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;

namespace Cyrena.Angular.Services
{
    internal class ComponentFolderWatcher : IStartupTask
    {
        private readonly IDevelopPlanService _planService;
        private IDisposable? _subscription;

        public ComponentFolderWatcher(IDevelopPlanService planService)
        {
            _planService = planService;
        }

        public int Order => 100;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _subscription = _planService.OnFileDeleted((file) =>
            {
                var plan = _planService.Plan;
                if (plan == null) return;

                // Determine the directory that contained the deleted file
                var fileDir = Path.GetDirectoryName(file.RelativePath);
                if (string.IsNullOrEmpty(fileDir)) return;

                var fullDirPath = Path.Combine(plan.RootDirectory, fileDir);

                // Directory must still exist on disk
                if (!Directory.Exists(fullDirPath)) return;

                // Must be completely empty — no files and no subdirectories
                if (Directory.GetFiles(fullDirPath).Length > 0 ||
                    Directory.GetDirectories(fullDirPath).Length > 0)
                    return;

                // Must be a component folder: parent directory is named "components"
                var parentDir = Path.GetDirectoryName(fileDir);
                if (string.IsNullOrEmpty(parentDir)) return;

                var parentName = Path.GetFileName(parentDir);
                if (!string.Equals(parentName, "components", StringComparison.OrdinalIgnoreCase))
                    return;

                // Remove the folder from the DevelopPlan
                var folder = FindFolderByRelativePath(plan, fileDir);
                if (folder != null)
                {
                    plan.RemoveFolder(folder, recursive: true);
                }

                // Delete the empty directory from disk
                try
                {
                    Directory.Delete(fullDirPath, recursive: false);
                }
                catch
                {
                    // Best-effort cleanup; folder is already gone from the plan
                }
            });

            return Task.CompletedTask;
        }

        private static DevelopFolder? FindFolderByRelativePath(DevelopPlan plan, string relativePath)
        {
            var normalized = NormalizePath(relativePath);
            return FindFolderRecursive(plan.Folders, normalized);
        }

        private static DevelopFolder? FindFolderRecursive(List<DevelopFolder> folders, string targetPath)
        {
            foreach (var folder in folders)
            {
                if (NormalizePath(folder.RelativePath) == targetPath)
                    return folder;

                var found = FindFolderRecursive(folder.Folders, targetPath);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static string NormalizePath(string path) =>
            path.Replace('\\', '/').Trim('/');
    }
}

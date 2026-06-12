using Cyrena.Coding.Models;

namespace Cyrena.Coding.Extensions;

public static class DevelopPlanExtensions
{
    /// <summary>
    /// Dynamically discovers the structure of a project
    /// </summary>
    /// <param name="plan"><see cref="DevelopPlan"/></param>
    /// <param name="extension">Extensions to look for like "py", "cs", "js", "html"</param>
    /// <param name="indexOnRoot">If this file should be indexed on root directory as well</param>
    /// <exception cref="ArgumentException">If <see cref="extension"/> contains invalid characters</exception>
    public static void Discover(this DevelopPlan plan, string extension, bool indexOnRoot, bool readOnly)
    {
        if(extension.Contains('.') || extension.Contains('*'))
            throw new ArgumentException($"{extension} is not in correct format. Use 'cs' and not '*.cs' for example.");
        var files = Directory.GetFiles(plan.RootDirectory, $"*.{extension}", SearchOption.AllDirectories);
        var relativeFilePaths = files.Select(x => x.Replace(plan.RootDirectory, "").Replace(@"\", "/"));

        // Collect all unique directory segments and order them root-first
        var orderedDirectories = relativeFilePaths
            .Select(x => Path.GetDirectoryName(x))
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .Select(x => x!.Trim('/').Split('/'))
            .SelectMany(segments => Enumerable.Range(1, segments.Length)
                .Select(i => segments.Take(i).ToArray()))
            .DistinctBy(x => string.Join("/", x))
            .OrderBy(x => x.Length)
            .ThenBy(x => string.Join("/", x))
            .ToList();

        var nodeMap = new Dictionary<string, DirectoryNode>();
        var roots = new List<DirectoryNode>();

        foreach (var segments in orderedDirectories)
        {
            var path = string.Join("/", segments);
            var node = new DirectoryNode
            {
                Id   = string.Join("_", segments.Select(s => s.ToLower())),
                Name = segments.Last(),
                Path = path
            };

            if (string.IsNullOrEmpty(node.Name) || node.Name.StartsWith('.')) continue;
            nodeMap[path] = node;

            if (segments.Length == 1)
            {
                roots.Add(node);
            }
            else
            {
                var parentPath = string.Join("/", segments.SkipLast(1));
                nodeMap[parentPath].Children.Add(node);
            }
        }

        foreach (var node in roots)
            Traverse(plan, node, extension, readOnly);
        if (indexOnRoot)
            plan.IndexFiles(extension, $"root_{extension}_", readOnly);
    }
    
    public static void Discover(this DevelopPlan plan, DevelopFolder folder, string extension, bool readOnly)
    {
        if(extension.Contains('.') || extension.Contains('*'))
            throw new ArgumentException($"{extension} is not in correct format. Use 'cs' and not '*.cs' for example.");
        if(folder.IsVirtual) return;
        var files = Directory.GetFiles(Path.Combine(plan.RootDirectory, folder.RelativePath), $"*.{extension}", SearchOption.AllDirectories);
        var relativeFilePaths = files.Select(x => x.Replace(plan.RootDirectory, "").Replace(@"\", "/"));

        // Collect all unique directory segments and order them root-first
        var orderedDirectories = relativeFilePaths
            .Select(x => Path.GetDirectoryName(x))
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .Select(x => x!.Trim('/').Split('/'))
            .SelectMany(segments => Enumerable.Range(1, segments.Length)
                .Select(i => segments.Take(i).ToArray()))
            .DistinctBy(x => string.Join("/", x))
            .OrderBy(x => x.Length)
            .ThenBy(x => string.Join("/", x))
            .ToList();

        var nodeMap = new Dictionary<string, DirectoryNode>();
        var roots = new List<DirectoryNode>();

        foreach (var segments in orderedDirectories)
        {
            var path = string.Join("/", segments);
            var node = new DirectoryNode
            {
                Id   = string.Join("_", segments.Select(s => s.ToLower())),
                Name = segments.Last(),
                Path = path
            };

            if (string.IsNullOrEmpty(node.Name) || plan.IgnoredDirectories.Contains(node.Name) || node.Name.StartsWith('.')) continue;
            nodeMap[path] = node;

            if (segments.Length == 1)
            {
                roots.Add(node);
            }
            else
            {
                var parentPath = string.Join("/", segments.SkipLast(1));
                nodeMap[parentPath].Children.Add(node);
            }
        }

        foreach (var node in roots)
            Traverse(plan, node, extension, readOnly);
        plan.IndexFiles(folder, extension, $"{folder.Id}_", readOnly);
    }

    public static bool ContainsFileTypes(this DevelopPlan plan, string extension)
    {
        if(!Directory.Exists(plan.RootDirectory))
            return false;
        var files = Directory.GetFiles(plan.RootDirectory, $"*.{extension}", SearchOption.AllDirectories);
        return files.Length != 0;
    }
    
    private static void Traverse(DevelopPlan plan, DirectoryNode node, string extension, bool readOnly)
    {
        if(plan.IgnoredDirectories.Contains(node.Name))
            return;
        var folder = plan.GetOrCreateFolder(node.Id, node.Name);
        plan.IndexFiles(folder, extension, $"{node.Id}_", readOnly);
        foreach (var child in node.Children)
            Traverse(plan, child, folder, extension, readOnly);
    }

    private static void Traverse(DevelopPlan plan, DirectoryNode node, DevelopFolder parent, string extension, bool readOnly)
    {
        if(plan.IgnoredDirectories.Contains(node.Name))
            return;
        var folder = plan.GetOrCreateFolder(parent, node.Id, node.Name);
        plan.IndexFiles(folder, extension, $"{node.Id}_", readOnly);
        foreach (var child in node.Children)
            Traverse(plan, child, folder, extension, readOnly);
    }
}
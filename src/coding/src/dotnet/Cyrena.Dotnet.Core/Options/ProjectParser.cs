using Cyrena.Dotnet.Models;
using System.Xml.Linq;

namespace Cyrena.Dotnet.Options
{
    public class ProjectParser
    {
        private static readonly HashSet<string> SupportedExtensions = [".csproj", ".fsproj"];

        /// <summary>
        /// Parses a .csproj or .fsproj file and extracts project information
        /// </summary>
        public static ProjectFileInfo ParseProject(string projectPath)
        {
            if (!File.Exists(projectPath))
                throw new FileNotFoundException($"Project file not found: {projectPath}");

            var extension = Path.GetExtension(projectPath).ToLowerInvariant();
            if (!SupportedExtensions.Contains(extension))
                throw new ArgumentException($"Unsupported project type: {extension}");

            var projectInfo = new ProjectFileInfo
            {
                FilePath = Path.GetFullPath(projectPath),
                FileName = Path.GetFileName(projectPath),
                IsFSharp = extension == ".fsproj"
            };

            try
            {
                XDocument doc = XDocument.Load(projectPath);
                XElement? root = doc.Root;

                if (root == null)
                    throw new InvalidOperationException("Invalid project file: no root element");

                XAttribute? sdkAttribute = root.Attribute("Sdk");
                projectInfo.IsSdkStyle = sdkAttribute != null;
                projectInfo.SdkType = sdkAttribute?.Value;

                XElement? rootNamespaceElement = root.Descendants("RootNamespace").FirstOrDefault();
                if (rootNamespaceElement != null && !string.IsNullOrWhiteSpace(rootNamespaceElement.Value))
                {
                    projectInfo.RootNamespace = rootNamespaceElement.Value.Trim();
                    projectInfo.IsRootNamespaceExplicit = true;
                }
                else
                {
                    projectInfo.RootNamespace = Path.GetFileNameWithoutExtension(projectPath);
                    projectInfo.IsRootNamespaceExplicit = false;
                }

                projectInfo.TargetFrameworks = ExtractTargetFrameworks(root);
                projectInfo.NuGetPackages = ExtractNuGetPackages(root);
                projectInfo.FrameworkReferences = ExtractFrameworkReferences(root);
                projectInfo.OutputType = ExtractOutputType(root);

                if (projectInfo.IsFSharp)
                    projectInfo.CompileOrder = ExtractCompileOrder(root);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse project file: {projectPath}", ex);
            }

            return projectInfo;
        }

        /// <summary>
        /// Gets the ordered list of <Compile> entries from an fsproj
        /// </summary>
        public static List<string> GetCompileOrder(string fsprojPath)
        {
            if (!File.Exists(fsprojPath))
                throw new FileNotFoundException($"Project file not found: {fsprojPath}");

            if (!fsprojPath.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("GetCompileOrder is only valid for .fsproj files");

            XDocument doc = XDocument.Load(fsprojPath);
            return ExtractCompileOrder(doc.Root!);
        }

        /// <summary>
        /// Rebuilds the <Compile> block in an fsproj with the provided ordered relative paths
        /// </summary>
        public static void SetCompileOrder(string fsprojPath, IEnumerable<string> orderedRelativePaths)
        {
            if (!File.Exists(fsprojPath))
                throw new FileNotFoundException($"Project file not found: {fsprojPath}");

            if (!fsprojPath.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("SetCompileOrder is only valid for .fsproj files");

            XDocument doc = XDocument.Load(fsprojPath);
            XElement? root = doc.Root;

            if (root == null)
                throw new InvalidOperationException("Invalid fsproj: no root element");

            var existingItemGroup = root.Descendants("Compile").FirstOrDefault()?.Parent;

            if (existingItemGroup != null)
            {
                existingItemGroup.Elements("Compile").Remove();
                foreach (var path in orderedRelativePaths)
                    existingItemGroup.Add(new XElement("Compile", new XAttribute("Include", path)));
            }
            else
            {
                var itemGroup = new XElement("ItemGroup");
                foreach (var path in orderedRelativePaths)
                    itemGroup.Add(new XElement("Compile", new XAttribute("Include", path)));
                root.Add(itemGroup);
            }

            doc.Save(fsprojPath);
        }

        private static List<string> ExtractCompileOrder(XElement root)
        {
            return root
                .Descendants("Compile")
                .Select(x => x.Attribute("Include")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        private static string ExtractTargetFrameworks(XElement root)
        {
            var targetFrameworksElement = root.Descendants("TargetFrameworks").FirstOrDefault();
            if (targetFrameworksElement != null && !string.IsNullOrWhiteSpace(targetFrameworksElement.Value))
                return targetFrameworksElement.Value.Trim().Replace(";", ", ");

            var targetFrameworkElement = root.Descendants("TargetFramework").FirstOrDefault();
            if (targetFrameworkElement != null && !string.IsNullOrWhiteSpace(targetFrameworkElement.Value))
                return targetFrameworkElement.Value.Trim();

            var targetFrameworkVersionElement = root.Descendants("TargetFrameworkVersion").FirstOrDefault();
            if (targetFrameworkVersionElement != null && !string.IsNullOrWhiteSpace(targetFrameworkVersionElement.Value))
                return targetFrameworkVersionElement.Value.Trim();

            return string.Empty;
        }

        private static List<NuGetPackage> ExtractNuGetPackages(XElement root)
        {
            var packages = new List<NuGetPackage>();
            foreach (var packageRef in root.Descendants("PackageReference"))
            {
                var includeAttr = packageRef.Attribute("Include");
                if (includeAttr == null || string.IsNullOrWhiteSpace(includeAttr.Value))
                    continue;

                var package = new NuGetPackage { Name = includeAttr.Value.Trim() };

                var versionAttr = packageRef.Attribute("Version");
                if (versionAttr != null && !string.IsNullOrWhiteSpace(versionAttr.Value))
                    package.Version = versionAttr.Value.Trim();
                else
                {
                    var versionElement = packageRef.Element("Version");
                    if (versionElement != null && !string.IsNullOrWhiteSpace(versionElement.Value))
                        package.Version = versionElement.Value.Trim();
                }

                packages.Add(package);
            }
            return packages;
        }

        private static List<string> ExtractFrameworkReferences(XElement root)
        {
            return root.Descendants("FrameworkReference")
                .Select(x => x.Attribute("Include")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        private static string? ExtractOutputType(XElement root)
        {
            var outputTypeElement = root.Descendants("OutputType").FirstOrDefault();
            return !string.IsNullOrWhiteSpace(outputTypeElement?.Value)
                ? outputTypeElement!.Value.Trim()
                : null;
        }

        public static bool IsSdkStyleProject(string projectPath)
        {
            if (!File.Exists(projectPath))
                throw new FileNotFoundException($"Project file not found: {projectPath}");

            try
            {
                XDocument doc = XDocument.Load(projectPath);
                return doc.Root?.Attribute("Sdk") != null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check project file: {projectPath}", ex);
            }
        }

        public static string GetRootNamespace(string projectPath)
            => ParseProject(projectPath).RootNamespace;

        public static string GetTargetFrameworks(string projectPath)
            => ParseProject(projectPath).TargetFrameworks;

        public static List<NuGetPackage> GetNuGetPackages(string projectPath)
            => ParseProject(projectPath).NuGetPackages;

        public static List<string> GetFrameworkReferences(string projectPath)
            => ParseProject(projectPath).FrameworkReferences;
    }
}
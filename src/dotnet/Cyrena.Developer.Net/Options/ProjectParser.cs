using Cyrena.Developer.Models;
using System.Xml.Linq;

namespace Cyrena.Developer.Options
{
    public class ProjectParser
    {
        /// <summary>
        /// Parses a .csproj file and extracts project information
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>ProjectFileInfo containing SDK type and root namespace</returns>
        public static ProjectFileInfo ParseProject(string csprojPath)
        {
            if (!File.Exists(csprojPath))
            {
                throw new FileNotFoundException($"Project file not found: {csprojPath}");
            }

            var projectInfo = new ProjectFileInfo
            {
                FilePath = Path.GetFullPath(csprojPath),
                FileName = Path.GetFileName(csprojPath)
            };

            try
            {
                XDocument doc = XDocument.Load(csprojPath);
                XElement? root = doc.Root;

                if (root == null)
                {
                    throw new InvalidOperationException("Invalid project file: no root element");
                }

                // Check if this is an SDK-style project
                XAttribute? sdkAttribute = root.Attribute("Sdk");
                projectInfo.IsSdkStyle = sdkAttribute != null;
                projectInfo.SdkType = sdkAttribute?.Value;

                // Extract RootNamespace
                XElement? rootNamespaceElement = root.Descendants("RootNamespace").FirstOrDefault();

                if (rootNamespaceElement != null && !string.IsNullOrWhiteSpace(rootNamespaceElement.Value))
                {
                    // RootNamespace is explicitly set
                    projectInfo.RootNamespace = rootNamespaceElement.Value.Trim();
                    projectInfo.IsRootNamespaceExplicit = true;
                }
                else
                {
                    // RootNamespace not set - use project file name without extension
                    projectInfo.RootNamespace = Path.GetFileNameWithoutExtension(csprojPath);
                    projectInfo.IsRootNamespaceExplicit = false;
                }

                // Extract Target Framework(s)
                projectInfo.TargetFrameworks = ExtractTargetFrameworks(root);

                // Extract NuGet packages
                projectInfo.NuGetPackages = ExtractNuGetPackages(root);

                // Extract Framework references
                projectInfo.FrameworkReferences = ExtractFrameworkReferences(root);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse project file: {csprojPath}", ex);
            }

            return projectInfo;
        }

        /// <summary>
        /// Extracts target framework(s) as a single string
        /// </summary>
        private static string ExtractTargetFrameworks(XElement root)
        {
            // Try TargetFrameworks (plural) first - used for multi-targeting
            var targetFrameworksElement = root.Descendants("TargetFrameworks").FirstOrDefault();
            if (targetFrameworksElement != null && !string.IsNullOrWhiteSpace(targetFrameworksElement.Value))
            {
                // Already in the format we want: "net8.0;net9.0;net10.0"
                // Convert semicolons to comma-space for readability
                return targetFrameworksElement.Value.Trim().Replace(";", ", ");
            }

            // Try TargetFramework (singular) - used for single target
            var targetFrameworkElement = root.Descendants("TargetFramework").FirstOrDefault();
            if (targetFrameworkElement != null && !string.IsNullOrWhiteSpace(targetFrameworkElement.Value))
            {
                return targetFrameworkElement.Value.Trim();
            }

            // Legacy format: TargetFrameworkVersion (e.g., "v4.7.2")
            var targetFrameworkVersionElement = root.Descendants("TargetFrameworkVersion").FirstOrDefault();
            if (targetFrameworkVersionElement != null && !string.IsNullOrWhiteSpace(targetFrameworkVersionElement.Value))
            {
                return targetFrameworkVersionElement.Value.Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// Extracts NuGet package references
        /// </summary>
        private static List<NuGetPackage> ExtractNuGetPackages(XElement root)
        {
            var packages = new List<NuGetPackage>();

            // SDK-style projects use <PackageReference>
            var packageReferences = root.Descendants("PackageReference");
            foreach (var packageRef in packageReferences)
            {
                var includeAttr = packageRef.Attribute("Include");
                if (includeAttr != null && !string.IsNullOrWhiteSpace(includeAttr.Value))
                {
                    var package = new NuGetPackage
                    {
                        Name = includeAttr.Value.Trim()
                    };

                    // Version can be in attribute or child element
                    var versionAttr = packageRef.Attribute("Version");
                    if (versionAttr != null && !string.IsNullOrWhiteSpace(versionAttr.Value))
                    {
                        package.Version = versionAttr.Value.Trim();
                    }
                    else
                    {
                        var versionElement = packageRef.Element("Version");
                        if (versionElement != null && !string.IsNullOrWhiteSpace(versionElement.Value))
                        {
                            package.Version = versionElement.Value.Trim();
                        }
                    }

                    packages.Add(package);
                }
            }

            // Legacy projects use packages.config, but also can have <Reference> with HintPath pointing to packages folder
            // For now, we'll just handle PackageReference which covers most modern scenarios

            return packages;
        }

        /// <summary>
        /// Extracts framework references
        /// </summary>
        private static List<string> ExtractFrameworkReferences(XElement root)
        {
            var frameworkRefs = new List<string>();

            var frameworkReferences = root.Descendants("FrameworkReference");
            foreach (var frameworkRef in frameworkReferences)
            {
                var includeAttr = frameworkRef.Attribute("Include");
                if (includeAttr != null && !string.IsNullOrWhiteSpace(includeAttr.Value))
                {
                    frameworkRefs.Add(includeAttr.Value.Trim());
                }
            }

            return frameworkRefs;
        }

        /// <summary>
        /// Checks if a project is SDK-style (without parsing full details)
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>True if the project uses SDK attribute</returns>
        public static bool IsSdkStyleProject(string csprojPath)
        {
            if (!File.Exists(csprojPath))
            {
                throw new FileNotFoundException($"Project file not found: {csprojPath}");
            }

            try
            {
                XDocument doc = XDocument.Load(csprojPath);
                return doc.Root?.Attribute("Sdk") != null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check project file: {csprojPath}", ex);
            }
        }

        /// <summary>
        /// Gets the root namespace (explicit or inferred)
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>The root namespace</returns>
        public static string GetRootNamespace(string csprojPath)
        {
            var projectInfo = ParseProject(csprojPath);
            return projectInfo.RootNamespace;
        }

        /// <summary>
        /// Gets the target frameworks as a string
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>Target frameworks string (e.g., "net8.0" or "net8.0, net9.0")</returns>
        public static string GetTargetFrameworks(string csprojPath)
        {
            var projectInfo = ParseProject(csprojPath);
            return projectInfo.TargetFrameworks;
        }

        /// <summary>
        /// Gets the list of NuGet packages
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>List of NuGet packages</returns>
        public static List<NuGetPackage> GetNuGetPackages(string csprojPath)
        {
            var projectInfo = ParseProject(csprojPath);
            return projectInfo.NuGetPackages;
        }

        /// <summary>
        /// Gets the list of framework references
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file</param>
        /// <returns>List of framework reference names</returns>
        public static List<string> GetFrameworkReferences(string csprojPath)
        {
            var projectInfo = ParseProject(csprojPath);
            return projectInfo.FrameworkReferences;
        }
    }
}
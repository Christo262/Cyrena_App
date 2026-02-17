namespace Cyrena.Developer.Models
{
    /// <summary>
    /// Represents information extracted from a project file
    /// </summary>
    public class ProjectFileInfo
    {
        /// <summary>
        /// Full path to the project file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// File name of the project
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is an SDK-style project (has Sdk attribute)
        /// </summary>
        public bool IsSdkStyle { get; set; }

        /// <summary>
        /// The SDK type (e.g., "Microsoft.NET.Sdk", "Microsoft.NET.Sdk.Web")
        /// Null if not an SDK-style project
        /// </summary>
        public string? SdkType { get; set; }

        /// <summary>
        /// The root namespace for the project
        /// If not explicitly set, defaults to the project file name without extension
        /// </summary>
        public string RootNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Whether the RootNamespace was explicitly defined in the .csproj
        /// False means it was inferred from the file name
        /// </summary>
        public bool IsRootNamespaceExplicit { get; set; }

        /// <summary>
        /// Target framework(s) as a single string
        /// Examples: "net8.0" or "net8.0, net9.0, net10.0"
        /// </summary>
        public string TargetFrameworks { get; set; } = string.Empty;

        /// <summary>
        /// List of NuGet package references
        /// </summary>
        public List<NuGetPackage> NuGetPackages { get; set; } = new List<NuGetPackage>();

        /// <summary>
        /// Gets package names as a simple string array
        /// </summary>
        public string[] GetPackageNames()
        {
            return NuGetPackages.Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets packages with versions as a string array (e.g., "Newtonsoft.Json 13.0.1")
        /// </summary>
        public string[] GetPackagesWithVersions()
        {
            return NuGetPackages.Select(p => p.ToString()).ToArray();
        }

        /// <summary>
        /// Returns a summary of the project information
        /// </summary>
        public override string ToString()
        {
            string sdkInfo = IsSdkStyle ? $"SDK: {SdkType}" : "Legacy project";
            string namespaceInfo = IsRootNamespaceExplicit
                ? $"RootNamespace: {RootNamespace} (explicit)"
                : $"RootNamespace: {RootNamespace} (inferred)";
            string targetInfo = !string.IsNullOrWhiteSpace(TargetFrameworks)
                ? $"Target: {TargetFrameworks}"
                : "No target framework";
            string packageInfo = NuGetPackages.Count > 0
                ? $"{NuGetPackages.Count} packages"
                : "No packages";

            return $"{FileName} - {sdkInfo}, {namespaceInfo}, {targetInfo}, {packageInfo}";
        }
    }

    /// <summary>
    /// Represents a NuGet package reference
    /// </summary>
    public class NuGetPackage
    {
        /// <summary>
        /// Package name/ID
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Package version (may be empty if not specified)
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Returns a string representation of the package
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Version)
                ? Name
                : $"{Name} {Version}";
        }
    }
}

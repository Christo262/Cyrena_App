namespace Cyrena.Extensa.Loader.Models
{
    public class DependencyVersionException : Exception
    {
        public DependencyVersionException(string extensionId, Version extensionVersion, string dependencyId, Version dependencyVersion)
            : base($"{extensionId} requires {dependencyId} with min version {dependencyVersion}")
        {
            ExtensionId = extensionId;
            ExtensionVersion = extensionVersion;
            DependencyId = dependencyId;
            DependencyVersion = dependencyVersion;
        }

        public string ExtensionId { get; set; }
        public Version ExtensionVersion { get; set; }
        public string DependencyId { get; set; }
        public Version DependencyVersion { get; set; }
    }
}

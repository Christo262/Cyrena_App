namespace Cyrena.Extensa.Loader.Models
{
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException(string extensionId, Version extensionVersion, string dependencyId, Version dependencyVersion)
            : base($"{extensionId} has circular dependency with {dependencyId}")
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

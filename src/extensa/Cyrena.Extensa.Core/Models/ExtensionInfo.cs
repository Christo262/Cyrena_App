namespace Cyrena.Extensa.Models
{
    public class ExtensionInfo
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public Version Version { get; set; } = Version.Parse("1.0.0");
        public string? EntryAssemblyFile { get; set; }
        public string? ContentRootDirectory { get; set; }
        public Dependency[] Dependencies { get; set; } = [];
        public bool RequireFrameworkBuilder { get; set; } = true;
    }
}

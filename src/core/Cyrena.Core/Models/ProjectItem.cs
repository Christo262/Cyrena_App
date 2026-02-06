namespace Cyrena.Models
{
    public abstract class ProjectItem : JsonStringObject
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string RelativePath { get; set; } = default!;
    }
}

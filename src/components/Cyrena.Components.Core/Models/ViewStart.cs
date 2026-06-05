namespace Cyrena.Models
{
    /// <summary>
    /// Information for a configurable starting view
    /// </summary>
    public sealed class ViewStart
    {
        public ViewStart(string id, Type componentType, string title, string? description)
        {
            Id = id;
            ComponentType = componentType;
            Title = title;
            Description = description;
        }
        public string Id { get; }
        public Type ComponentType { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}

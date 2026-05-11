namespace Cyrena.Models
{
    /// <summary>
    /// Information for a configurable starting view
    /// </summary>
    public sealed class ViewStart
    {
        public string Href { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
    }
}

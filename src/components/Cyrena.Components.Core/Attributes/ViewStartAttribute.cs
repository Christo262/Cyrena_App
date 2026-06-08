namespace Cyrena.Attributes;

/// <summary>
/// Marks a view as a possible starting view
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewStartAttribute : Attribute
{
    public ViewStartAttribute(string id, string title, string? description = null)
    {
        Id = id;
        Title = title;
        Description = description;
    }
    public string Id { get; }
    public string Title { get; }
    public string? Description { get; }
}
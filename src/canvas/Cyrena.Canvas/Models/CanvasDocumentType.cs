namespace Cyrena.Canvas.Models
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum CanvasDocumentType
    {
        Text,
        Html,
        Markdown
    }
}

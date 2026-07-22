using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelContentExtensions
    {
#pragma warning disable SKEXP0110
        public static TextContent ToTextContent(this FileReferenceContent reference)
        {
            var text = $"[Attachment: {reference.FileId}, Content Type: {reference.MimeType}, Tools: {string.Join(", ", reference.Tools ?? Array.Empty<string>())}]";
            return new TextContent(text);
        }
#pragma warning restore SKEXP0110
    }
}

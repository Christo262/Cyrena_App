using Microsoft.SemanticKernel;

namespace Cyrena.Models
{
    /// <summary>
    /// Purely for display purposes, nothing more
    /// </summary>
    public sealed class InfoMessageContentItem : KernelContent
    {
        public InfoMessageContentItem() { }
        public InfoMessageContentItem(string fileName)
        {
            FileName = fileName;
        }
        public string FileName { get; set; } = default!;
    }
}

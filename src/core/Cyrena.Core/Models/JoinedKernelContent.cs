using Microsoft.SemanticKernel;

namespace Cyrena.Models;

/// <summary>
/// Used to simplify file attachment reference and content sending in one. 
/// </summary>
public sealed class JoinedKernelContent : KernelContent
{
    public JoinedKernelContent(KernelContent[] contents, KernelContent? saveAs)
    {
        Contents = contents;
        var metadata = new Dictionary<string, object?>()
        {
            ["save-as"] = saveAs
        };
        if (saveAs is { Metadata: not null })
            foreach(var item in saveAs.Metadata)
                metadata[item.Key] = item.Value;
        Metadata = metadata;
        SaveAs = saveAs;
    }
    public KernelContent[] Contents { get; }
    public KernelContent? SaveAs { get; }
}
using Cyrena.Extensa.Models;
using Cyrena.Options;
using Cyrena.VisualStudio.Extensions;
using Cyrena.VisualStudio.Services;

namespace Cyrena.VisualStudio;

public class VisualStudioExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        builder.AddProjectHandler<CsProjHandler>();
        builder.AddProjectHandler<FsProjHandler>();
        builder.AddProjectHandler<EsProjHandler>();
    }
}
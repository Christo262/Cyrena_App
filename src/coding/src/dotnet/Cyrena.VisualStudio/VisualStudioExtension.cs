using Cyrena.Coding.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.VisualStudio.Extensions;
using Cyrena.VisualStudio.Models;
using Cyrena.VisualStudio.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.VisualStudio;

public class VisualStudioExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        builder.AddProjectHandler<CsProjHandler>();
        builder.AddProjectHandler<FsProjHandler>();
        builder.AddProjectHandler<EsProjHandler>();
        builder.Services.AddSingleton<ICodeBuilder, SlnCodeBuilder>();
        builder.AddShortcut<SolutionShortcut>();
    }
}
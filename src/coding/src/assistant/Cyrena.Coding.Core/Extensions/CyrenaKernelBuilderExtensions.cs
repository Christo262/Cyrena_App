using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Coding.Services;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Coding.Extensions;

public static class CyrenaKernelBuilderExtensions
{
    public static CyrenaKernelBuilder UseDynamicDiscovery(this CyrenaKernelBuilder builder, Action<DevelopPlan> initialization)
    {
        var options = builder.TryGetDynamicOptions();
        if(options != null)
            throw new InvalidOperationException("Dynamic discovery is already configured");
        options = new DynamicDiscoveryOptions()
        {
            Initialization = initialization
        };
        builder.Services.AddSingleton(options);
        builder.Plugins.AddFromType<DynamicFileFunctions>("FS");
        return builder;
    }

    public static DynamicDiscoveryOptions? TryGetDynamicOptions(this CyrenaKernelBuilder builder)
    {
        try
        {
            return builder.GetFeatureOption<DynamicDiscoveryOptions>();
        }
        catch
        {
            return null;
        }
    }
}
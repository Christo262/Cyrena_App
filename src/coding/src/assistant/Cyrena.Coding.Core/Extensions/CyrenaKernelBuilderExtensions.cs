using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

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
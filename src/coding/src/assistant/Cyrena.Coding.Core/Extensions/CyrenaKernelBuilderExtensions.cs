using Cyrena.Coding.Contracts;
using Cyrena.Coding.Services;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Coding.Extensions;

public static class CyrenaKernelBuilderExtensions
{
    public static CyrenaKernelBuilder UseDynamicDiscovery<TDynamicPlanInitializer>(this CyrenaKernelBuilder builder)
    where TDynamicPlanInitializer : class, IDynamicPlanInitializer
    {
        builder.Services.AddSingleton<IDynamicPlanInitializer,TDynamicPlanInitializer>();
        builder.KernelBuilder.AddStartupTask<DynamicDevelopPlanWatcher>();
        builder.Plugins.AddFromType<DynamicFileFunctions>("FS");
        return builder;
    }
}
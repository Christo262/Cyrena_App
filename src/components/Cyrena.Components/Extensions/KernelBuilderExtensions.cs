using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        public static void AddCapability<TComponent>(this IKernelBuilder builder)
            where TComponent : CapabilityComponentBase
        {
            var cap = new Capability(typeof(TComponent));
            builder.Services.AddSingleton<ICapability>(cap);
        }

        public static void AddToolbarComponent<TComponent>(this IKernelBuilder builder, ToolbarAlignment alignment)
            where TComponent : KernelComponentBase
        {
            builder.Services.AddSingleton<IToolbarComponent>(new ToolbarComponent(typeof(TComponent), alignment));
        }
    }
}

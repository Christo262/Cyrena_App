using Cyrena.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        public static void AddStartupTask<TStartupTask>(this IKernelBuilder builder)
            where TStartupTask: class, IStartupTask
        {
            builder.Services.AddScoped<IStartupTask, TStartupTask>();
        }
    }
}

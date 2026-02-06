using Cyrena.Contracts;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Spec.Services
{
    internal class SpecsExtension : IDeveloperContextExtension
    {
        public int Priority => 5;

        public Task ExtendAsync(IDeveloperContextBuilder builder)
        {
            builder.Services.AddSingleton<ISpecsService, SpecsService>();
            builder.Plugins.AddFromType<SpecsPlugin>();

            return Task.CompletedTask;
        }
    }
}

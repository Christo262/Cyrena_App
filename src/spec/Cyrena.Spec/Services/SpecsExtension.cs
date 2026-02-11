using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
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
            builder.AddStore<Article>("specs");
            builder.Services.AddSingleton<ISpecsService, SpecsService>();
            builder.Plugins.AddFromType<ProjectSpecifications>();
            return Task.CompletedTask;
        }
    }
}

using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Services;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Dotnet.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static void AddSolutionController(this CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<ISolutionController, SolutionController>();
        }

        public static void AddSolutionControllerWithProjectOverride(this CyrenaKernelBuilder builder)
        {
            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();
            persistence.AddSingletonStore<ProjectTypeOverride>("project-config");
            builder.Services.AddSingleton<ISolutionController, SolutionControllerOverride>();
            builder.KernelBuilder.AddStartupTask<SolutionOverrideStartupTask>();
        }

        public static void AddDynamicSolutionController(this CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<ISolutionController, DynamicSolutionController>();
        }
    }
}

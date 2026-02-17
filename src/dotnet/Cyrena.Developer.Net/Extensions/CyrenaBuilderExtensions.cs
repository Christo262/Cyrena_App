using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Developer.Services;
using Cyrena.Net.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddDotnetDevelopment(this CyrenaBuilder builder)
        {
            builder.AddSingletonStore<ProjectModel>("dotnet_projects");
            builder.Services.AddSingleton<ISolutionBuilder, SolutionBuilder>();
            builder.Services.AddSingleton<IDotnetProjectType, CSharpClassLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorAppProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, MvcProjectType>();

            builder.Services.AddSingleton<ISolutionBuilder, BlazorAppSolutionBuilder>();
            builder.Services.AddSingleton<ISolutionBuilder, ClassLibrarySolutionBuilder>();
            builder.Services.AddSingleton<ISolutionBuilder, MvcAppSolutionBuilder>();

            builder.AddShortcut<DotnetShortcut>();
            builder.AddShortcut<BlazorAppShortuct>();
            builder.AddShortcut<ClassLibraryShortcut>();
            builder.AddShortcut<MvcAppShortcut>();
            return builder;
        }
    }
}

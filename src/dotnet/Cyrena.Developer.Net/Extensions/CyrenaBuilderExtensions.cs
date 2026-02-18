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
            builder.Services.AddSingleton<ICodeBuilder, SolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, BlazorAppSolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, ClassLibrarySolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, MvcAppSolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, MvcLibrarySolutionBuilder>();

            builder.Services.AddSingleton<IDotnetProjectType, CSharpClassLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorAppProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, MvcProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, MvcLibraryProjectType>();

            builder.AddShortcut<DotnetShortcut>();
            builder.AddShortcut<BlazorAppShortuct>();
            builder.AddShortcut<ClassLibraryShortcut>();
            builder.AddShortcut<MvcAppShortcut>();
            builder.AddShortcut<MvcLibraryShortcut>();
            return builder;
        }
    }
}

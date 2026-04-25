using Cyrena.Coding.Contracts;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Services;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddDotnetDevelopment(this CyrenaBuilder builder)
        {
            builder.Services.AddSingleton<ICodeBuilder, SolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, ConsoleAppSolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, BlazorAppSolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, ClassLibrarySolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, MvcAppSolutionBuilder>();
            builder.Services.AddSingleton<ICodeBuilder, BlazorLibrarySolutionsBuilder>();

            builder.Services.AddSingleton<IDotnetProjectType, ConsoleAppProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, ClassLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorLibraryProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, BlazorAppProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, MvcProjectType>();
            builder.Services.AddSingleton<IDotnetProjectType, MvcLibraryProjectType>();

            builder.AddShortcut<DotnetShortcut>();
            builder.AddShortcut<ConsoleAppShortcut>();
            builder.AddShortcut<BlazorAppShortcut>();
            builder.AddShortcut<ClassLibraryShortcut>();
            builder.AddShortcut<MvcAppShortcut>();
            builder.AddShortcut<MvcLibraryShortcut>();
            builder.AddShortcut<BlazorLibraryShortcut>();
            return builder;
        }
    }
}

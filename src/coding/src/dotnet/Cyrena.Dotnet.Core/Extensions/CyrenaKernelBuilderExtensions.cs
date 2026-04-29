using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Services;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Dotnet.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static void AddSolutionController(this CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<ISolutionController, SolutionController>();
        }
    }
}

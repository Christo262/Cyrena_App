using Cyrena.Developer.Docs.Models;
using Cyrena.Developer.Docs.Plugins;
using Cyrena.Developer.Options;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class DevelopOptionsExtensions
    {
        public static void AddApiReferencing(this DevelopOptions options)
        {
            options.Persistence.AddSingletonStore<ApiReference>("api_references");
            options.Plugins.AddFromType<APIReferences>();
        }
    }
}

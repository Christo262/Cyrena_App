using Cyrena.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Platform.Tests.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Platform.Tests
{
    internal class PlatformTestsExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            throw new Exception("Failed to load this extension"); //Testing
            builder.AddAssistantPlugin<TestAssistantPlugin>();
            builder.Services.AddScoped<IViewStartProvider, ViewStartProvider>();
            builder.AddFeatureAssembly<PlatformTestsExtension>("blazor");
        }
    }
}

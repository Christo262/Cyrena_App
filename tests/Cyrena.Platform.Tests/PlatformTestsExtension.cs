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
            builder.AddAssistantPlugin<TestAssistantPlugin>();
            builder.AddFeatureAssembly<PlatformTestsExtension>("blazor");
        }
    }
}

using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Synthesis.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static CyrenaKernelBuilder AddCapabilityAbiDescriptor<T>(this CyrenaKernelBuilder builder, string instruction)
            where T : class
        {
            SynthesisBuilder synth;
            try
            {
                synth = builder.GetFeatureOption<SynthesisBuilder>();
            }
            catch //GetFeatureOption throws if null
            {
                synth = new SynthesisBuilder();
                builder.AddFeatureOption<SynthesisBuilder>(synth);
                builder.Services.AddSingleton(synth);
            }
            var dsc = new CapabilityAbiDescriptor(typeof(T), instruction);
            synth.AddDescriptor(dsc);
            return builder;
        }
    }
}

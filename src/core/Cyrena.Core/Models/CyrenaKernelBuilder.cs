using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Models
{
    /// <summary>
    /// Use to configure a new kernel instance
    /// </summary>
    public sealed class CyrenaKernelBuilder
    {
        public CyrenaKernelBuilder(ChatConfiguration chatConfiguration, IKernelBuilder kernelBuilder)
        {
            ChatConfiguration = chatConfiguration;
            KernelBuilder = kernelBuilder;
            FeatureOptions = new Dictionary<string, object>();
        }

        public ChatConfiguration ChatConfiguration { get; }
        public IKernelBuilder KernelBuilder { get; }
        public IDictionary<string, object> FeatureOptions { get; }
        public IServiceCollection Services => KernelBuilder.Services;
        public IKernelBuilderPlugins Plugins => KernelBuilder.Plugins;
    }
}

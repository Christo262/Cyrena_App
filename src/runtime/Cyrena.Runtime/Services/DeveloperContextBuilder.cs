using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Runtime.Services
{
    internal class DeveloperContextBuilder : IDeveloperContextBuilder
    {
        public DeveloperContextBuilder(IKernelBuilder builder, Project project)
        {
            KernelBuilder = builder;
            Project = project;
            KernelHistory = new ChatHistory();
            DisplayHistory = new ChatHistory();
            _options = new List<object>();
        }

        public IKernelBuilder KernelBuilder { get; }
        public IKernelBuilderPlugins Plugins => KernelBuilder.Plugins;
        public IServiceCollection Services => KernelBuilder.Services;
        public ChatHistory KernelHistory { get; }
        public ChatHistory DisplayHistory { get; }
        public Project Project { get; }

        private readonly List<object> _options;
        public void AddOption<TOption>(TOption value)
            where TOption : class
        {
            _options.Add(value);
        }

        public TOption? GetOption<TOption>()
            where TOption : class
        {
            foreach (var option in _options)
                if (option is TOption)
                    return (TOption?)option;
            return null;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IDeveloperContextBuilder
    {
        IKernelBuilder KernelBuilder { get; }
        IServiceCollection Services { get; }
        IKernelBuilderPlugins Plugins { get; }
        ChatHistory KernelHistory { get; }
        ChatHistory DisplayHistory { get; }
        Project Project { get; }
        void AddOption<TOption>(TOption value) where TOption : class;
        TOption? GetOption<TOption>() where TOption : class;
    }
}

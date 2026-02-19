using Cyrena.Models;
using Cyrena.Persistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Developer.Options
{
    public sealed class DevelopOptions
    {
        public const string AssistantModeId = "developer";
        public const string BuilderId = "dev.builder-id";
        public const string RootDirectory = "dev.root-dir";

        private readonly IKernelBuilder _builder;
        public DevelopOptions(IKernelBuilder builder, ICyrenaPersistenceBuilder persistence, ChatConfiguration config)
        {
            _builder = builder;
            ChatConfiguration = config;
            Persistence = persistence;
        }

        public ChatConfiguration ChatConfiguration { get;}
        public ICyrenaPersistenceBuilder Persistence { get; }
        public IServiceCollection Services => _builder.Services;
        public IKernelBuilderPlugins Plugins => _builder.Plugins;
        public IKernelBuilder KernelBuilder => _builder;
    }
}

using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    /// <summary>
    /// Registers usage tracker and startup task
    /// </summary>
    internal class AllAssistantsPlugin : IAssistantPlugin
    {
        private readonly IKernelController _store;
        public AllAssistantsPlugin(IKernelController store)
        {
            _store = store;
        }

        public string[] Modes => [];

        public int Priority => 10;

        public string Id => "cyrena.runtime";

        public bool Required => true;

        public string Title => "Runtime Services";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Plugins.AddFromType<Cyrena.Runtime.Plugins.DateTime>();
            builder.Plugins.AddFromType<Cyrena.Runtime.Plugins.Queue>();
            var config_service = new ChatConfigurationService(_store, builder.ChatConfiguration);
            builder.Services.AddSingleton<IChatConfigurationService>(config_service);
            builder.KernelBuilder.AddStartupTask<HistoryStartupTask>();
            var prompt = Resources.Read(typeof(AllAssistantsPlugin).Assembly, "Cyrena.Runtime.Resources.prompt-queue.md");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(5, prompt);

            var info = builder.GetFeatureOption<ConnectionInfo>();
            if (info.SupportImages)
                builder.Services.AddSingleton<IFileHandler, ImageFileHandler>();
            if (info.SupportFiles)
                builder.Services.AddSingleton<IFileHandler, TextFileHandler>();

            if(info.SupportFiles || info.SupportImages)
                builder.Plugins.AddFromType<AttachmentKernelFunctions>("Attachment");
            builder.Services.AddSingleton<IFileHandlerFactory, FileHandlerFactory>();
            builder.Services.AddSingleton<IConversationHistoryTransformer, FileReferenceContentTransformer>();

            builder.Services.AddSingleton<ICyrenaFileExporter, CyrenaFileExporter>();

            return Task.CompletedTask;
        }
    }

    internal class HistoryStartupTask : IStartupTask
    {
        private readonly IChatMessageService _srv;

        public HistoryStartupTask(IChatMessageService srv)
        {
            _srv = srv;
        }

        public int Order => 0;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await _srv.LoadHistoryAsync();
        }
    }
}

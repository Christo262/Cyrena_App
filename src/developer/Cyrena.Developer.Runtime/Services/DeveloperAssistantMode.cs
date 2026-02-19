using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Developer.Plugins;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Developer.Services
{
    internal class DeveloperAssistantMode : IAssistantMode
    {
        private readonly IServiceProvider _services;
        private readonly IKernelController _kernels;
        public DeveloperAssistantMode(IServiceProvider services, IKernelController kernels)
        {
            _services = services;
            _kernels = kernels;
        }

        public string Id => DevelopOptions.AssistantModeId;

        public async Task ConfigureAsync(ChatConfiguration config, IKernelBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                throw new InvalidOperationException($"{DevelopOptions.BuilderId} not set, unable to configure");
            if (string.IsNullOrEmpty(config[DevelopOptions.RootDirectory]) || !Directory.Exists(config[DevelopOptions.RootDirectory]))
                throw new InvalidOperationException($"{DevelopOptions.RootDirectory} not set, unable to configure");

            var sln_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if (sln_builder == null)
                throw new NullReferenceException($"Unable to find solution builder with id {config[DevelopOptions.BuilderId]}");

            var persistence = builder.AddFilePersistence(Path.Combine(config[DevelopOptions.RootDirectory]!, "./.cyrena"));
            builder.Services.Configure<ChatOptions>(o =>
            {
                o.AutoSave = false;
                o.IncludeLogsInDisplay = true;
            });
            builder.AddStartupTask<InstructStartupTask>();
            persistence.AddSingletonStore<StickyNote>("sticky_notes");
            var options = new DevelopOptions(builder, persistence, config);
            var plan = await sln_builder.ConfigureAsync(options);
            var plan_service = new DevelopPlanService(plan);
            builder.Services.AddSingleton<IDevelopPlanService>(plan_service);
            builder.Services.AddSingleton<IVersionControl, VersionControl>();
            builder.Plugins.AddFromType<FileActions>();
            builder.Plugins.AddFromType<ProjectInformation>();
            builder.AddToolbarComponent<VersionControlViewer>(ToolbarAlignment.Start);
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                return Task.CompletedTask;
            var sln_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if(sln_builder == null)
                return Task.CompletedTask;
            return sln_builder.DeleteAsync(config);
        }

        public Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                return Task.CompletedTask;
            var sln_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if (sln_builder == null)
                return services.GetRequiredService<DialogService>().ShowModal("Error", "Unable to find handler for this project type.", new ResultDialogOption()
                {
                    ButtonYesText = "Okay",
                    ShowNoButton = false,
                    Size = Size.Medium
                });
            return sln_builder.EditAsync(config, services);
        }
    }

    /// <summary>
    /// Changes message history behaviour to reduce context size per iteration
    /// </summary>
    internal class InstructStartupTask : IStartupTask
    {
        private readonly IIterationService _its;
        private readonly IChatMessageService _chat;
        private readonly ChatOptions _chatOptions;
        public InstructStartupTask(IIterationService its, IChatMessageService chat, IOptions<ChatOptions> chatOptions)
        {
            _its = its;
            _chat = chat;
            _chatOptions = chatOptions.Value;
        }

        public int Order => 10;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _its.OnIterationEnd(e =>
            {
                var hst = new ChatHistory();
                hst.AddRange(_chat.KernelHistory.Where(x => x.Role == _chatOptions.System));
                var usr = _chat.KernelHistory.LastOrDefault(x => x.Role == _chatOptions.User);
                var asst = _chat.KernelHistory.LastOrDefault(x => x.Role == _chatOptions.Assistant);
                var chat_hst = new List<ChatMessageContent>(_chat.DisplayHistory);
                if(usr is not null && asst is not null) //Only add last two messages to retain a little context. Keeping context light for file write operations
                {
                    hst.Add(usr);
                    hst.Add(asst);
                }
                _chat.LoadHistory(hst, chat_hst);
            });
            return Task.CompletedTask;
        }
    }
}

using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Components.Shared;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class DefaultAssistantMode : IAssistantMode
    {
        private readonly IStore<ChatMessage> _store;
        private readonly IKernelController _controller;
        public DefaultAssistantMode(IStore<ChatMessage> store, IKernelController controller)
        {
            _store = store;
            _controller = controller;
        }

        public string Id => IAssistantMode.AssistantModeDefault;

        public Task ConfigureAsync(CyrenaKernelBuilder builder)
        {
            builder.KernelBuilder.AddStartupTask<HistoryStartupTask>();
            builder.Services.Configure<ChatOptions>(o => { });
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(ChatConfiguration config)
        {
            await _store.DeleteManyAsync(x => x.ConversationId == config.Id);
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialog = services.GetRequiredService<DialogService>();
            var rf = await dialog.ShowModal<EditDefaultAssistant>(new ResultDialogOption()
            {
                Size = Size.Medium,
                Title = "Configure",
                ComponentParameters = new() { { nameof(EditDefaultAssistant.Model), config} }
            });
            if(rf == DialogResult.Yes)
                await _controller.UpdateAsync(config, true);
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
            if(_srv.KernelHistory.Count == 0)
            {
                var prompt = Resources.Read(typeof(HistoryStartupTask).Assembly, "Cyrena.Runtime.Resources.prompt.md");
                await _srv.AddSystemMessage(prompt);
            }
        }
    }
}

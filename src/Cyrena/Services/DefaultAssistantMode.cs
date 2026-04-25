using BootstrapBlazor.Components;
using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
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
            builder.Services.Configure<ChatOptions>(o => { });
            var prompts = builder.GetFeatureOption<IPromptManager>();
            var prompt = Resources.Read(typeof(DefaultAssistantMode).Assembly, "Cyrena.Resources.prompt.md");
            prompts.AddPrompt(0, prompt);
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
                ComponentParameters = new() { { nameof(EditDefaultAssistant.Model), config } }
            });
            if (rf == DialogResult.Yes)
                await _controller.UpdateAsync(config, true);
        }
    }
}

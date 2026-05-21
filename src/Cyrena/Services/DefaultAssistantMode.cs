using BootstrapBlazor.Components;
using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Services
{
    internal class DefaultAssistantMode : IAssistantMode
    {
        private readonly IKernelController _controller;
        public DefaultAssistantMode(IKernelController controller)
        {
            _controller = controller;
        }

        public string Id => IAssistantMode.AssistantModeDefault;

        public Task ConfigureAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.Configure<ChatOptions>(o => { });
            var prompts = builder.GetFeatureOption<IPromptManager>();
            var prompt = Resources.Read(typeof(DefaultAssistantMode).Assembly, "Cyrena.Resources.prompt.md");
            prompts.AddPrompt(0, prompt);
            builder.Plugins.AddFromType<Chat>();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
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

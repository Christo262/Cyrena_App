using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using MudBlazor;

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
            var dialog = services.GetRequiredService<IDialogService>();
            var parameters = new DialogParameters
            {
                { nameof(EditDefaultAssistant.Model), config }
            };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var result = await dialog.ShowAsync<EditDefaultAssistant>("Configure", parameters, options);
            var rf = await result.Result;
            if (rf is { Canceled:false})
                await _controller.UpdateAsync(config, true);
        }
    }
}

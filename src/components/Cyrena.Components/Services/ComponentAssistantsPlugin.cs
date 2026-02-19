using BootstrapBlazor.Components;
using Cyrena.Components.Tools;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Services
{
    internal class ComponentAssistantsPlugin : IAssistantPlugin
    {
        private readonly DialogService _dialog;
        private readonly ToastService _toasts;
        public ComponentAssistantsPlugin(DialogService dialog, ToastService toasts)
        {
            _dialog = dialog;
            _toasts = toasts;
        }
        public string[] Modes => [];

        public int Priority => 10;

        public Task LoadAsync(ChatConfiguration config, IKernelBuilder builder)
        {
            builder.Services.AddSingleton(_dialog);
            builder.Services.AddSingleton(_toasts);
            builder.AddToolbarComponent<ExportChat>(ToolbarAlignment.End);
            return Task.CompletedTask;
        }
    }
}

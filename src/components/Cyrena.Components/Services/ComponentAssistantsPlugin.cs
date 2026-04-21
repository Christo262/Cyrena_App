using BootstrapBlazor.Components;
using Cyrena.Components.Tools;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

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

        public string Id => "cyrena.components";

        public bool Required => true;

        public string Title => "UI Components";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton(_dialog);
            builder.Services.AddSingleton(_toasts);
            builder.KernelBuilder.AddToolbarComponent<ExportChat>(ToolbarAlignment.End);
            return Task.CompletedTask;
        }
    }
}

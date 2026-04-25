using Cyrena.Components.Tools;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ComponentAssistantsPlugin : IAssistantPlugin
    {
        public string[] Modes => [];

        public int Priority => 10;

        public string Id => "cyrena.components";

        public bool Required => true;

        public string Title => "UI Components";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IDisplayService, DisplayService>();
            builder.KernelBuilder.AddToolbarComponent<ExportChat>(ToolbarAlignment.End);
            builder.KernelBuilder.AddToolbarComponent<ClearChat>(ToolbarAlignment.End);
            builder.KernelBuilder.AddToolbarComponent<DisplayServiceComponent>(ToolbarAlignment.End);
            return Task.CompletedTask;
        }
    }
}

using Cyrena.Components.Tools;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ComponentAssistantsPlugin : IAssistantPlugin
    {
        private readonly IDisplayService _display;
        public ComponentAssistantsPlugin(IDisplayService display)
        {
            _display = display;
        }

        public string[] Modes => [];

        public int Priority => 10;

        public string Id => "cyrena.components";

        public bool Required => true;

        public string Title => "Display Components";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton<IDisplayService>(_display);
            builder.AddToolbarComponent<ExportChat>(ToolbarAlignment.End);
            builder.AddToolbarComponent<ClearChat>(ToolbarAlignment.End);
            var info = builder.GetFeatureOption<ConnectionInfo>();
            if(info.SupportFiles)
                builder.Services.AddSingleton<IFileHandler, PdfFileHandler>();
            return Task.CompletedTask;
        }
    }
}

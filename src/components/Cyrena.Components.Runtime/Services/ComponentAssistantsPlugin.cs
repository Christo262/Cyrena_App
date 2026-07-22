using Cyrena.Components.Shared;
using Cyrena.Components.Tools;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Services
{
    internal class ComponentAssistantsPlugin : IAssistantPlugin
    {
        public string[] Modes => [];

        public int Priority => 1000;

        public string Id => "cyrena.components";

        public bool Required => true;

        public string Title => "Display Components";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.AddToolbarComponent<ExportChat>(ToolbarAlignment.End);
            builder.AddToolbarComponent<ClearChat>(ToolbarAlignment.End);
            var info = builder.GetFeatureOption<ConnectionInfo>();
            if(info.SupportFiles)
                builder.Services.AddSingleton<IFileHandler, PdfFileHandler>();
            builder.Services.AddSingleton<IDockingService, DockingService>();
            builder.Services.AddScoped<IFileAttacher, FileUpload>();
            var overrides = new InterfaceOverrides();
            builder.AddFeatureOption<InterfaceOverrides>(overrides);
            builder.Services.AddSingleton(overrides);
            overrides.UseFileAttacher<FileUpload>();
            return Task.CompletedTask;
        }
    }
}

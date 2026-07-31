using Cyrena.Canvas.Components.Shared;
using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Canvas.Services
{
    internal class CanvasAssistantPlugin : IAssistantPlugin
    {
        public string Id => "cyrena.canvas";
        public string[] Modes => [];

        public int Priority => 20;

        public bool Required => false;

        public string Title => "Text Canvas";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.AddToolbarComponent<Toolbar>(ToolbarAlignment.End);
            builder.Services.AddSingleton<ICanvasService, CanvasService>();
            builder.Plugins.AddFromType<CanvasKernelFunctions>("Canvas");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(10, Resources.Read(typeof(CanvasAssistantPlugin).Assembly, "Cyrena.Canvas.Resources.prompt.md"));
            var info = builder.GetFeatureOption<ConnectionInfo>();
            return Task.CompletedTask;
        }
    }
}

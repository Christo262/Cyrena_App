using Cyrena.Canvas.Services;
using Cyrena.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Canvas
{
    public class CanvasExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddAssistantPlugin<CanvasAssistantPlugin>();
            builder.Services.AddSingleton<ICyrenaFileImporter, CanvasFileImporter>();
        }
    }
}

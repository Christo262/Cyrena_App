using Cyrena.Canvas.Services;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Canvas
{
    public class CanvasExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddAssistantPlugin<CanvasAssistantPlugin>();
        }
    }
}

using Cyrena.Canvas.Models;
using Cyrena.Canvas.Services;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Persistence.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Canvas
{
    public class CanvasExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddAssistantPlugin<CanvasAssistantPlugin>();

            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();
            persistence.AddSingletonStore<CanvasDocument>("canvas-documents");

            builder.AddStartupTask<CanvasStartupTask>();
        }
    }
}

using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Tavily
{
    public class TavilyExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddTavily();
        }
    }
}

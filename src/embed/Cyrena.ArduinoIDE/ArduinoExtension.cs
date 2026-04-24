using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.ArduinoIDE
{
    public class ArduinoExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddArduinoIDE();
        }
    }
}

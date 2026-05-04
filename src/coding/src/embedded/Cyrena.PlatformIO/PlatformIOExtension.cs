using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.PlatformIO
{
    public class PlatformIOExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddPlatformIO();
        }
    }
}

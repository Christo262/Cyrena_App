using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Developer
{
    public class DeveloperExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddDeveloperRuntime();
        }
    }
}

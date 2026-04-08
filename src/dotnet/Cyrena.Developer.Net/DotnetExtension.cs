using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Developer
{
    public class DotnetExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddDotnetDevelopment();
        }
    }
}

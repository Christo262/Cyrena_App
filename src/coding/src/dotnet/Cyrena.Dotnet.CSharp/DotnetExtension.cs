using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Dotnet
{
    public class DotnetExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddDotnetDevelopment();
        }
    }
}

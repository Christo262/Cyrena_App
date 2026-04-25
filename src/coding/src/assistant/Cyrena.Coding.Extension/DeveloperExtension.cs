using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Coding
{
    public class DeveloperExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddDeveloperRuntime();
        }
    }
}

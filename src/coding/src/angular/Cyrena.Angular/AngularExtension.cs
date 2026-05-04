using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Angular
{
    public class AngularExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddAngular();
        }
    }
}

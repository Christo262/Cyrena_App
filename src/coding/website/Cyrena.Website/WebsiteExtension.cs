using Cyrena.Extensa.Models;
using Cyrena.Options;
using Cyrena.Website.Extensions;

namespace Cyrena.Website
{
    public class WebsiteExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddWebsite();
        }
    }
}

using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Developer.Docs
{
    public class DocumentsExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddApiReferencePages();
        }
    }
}

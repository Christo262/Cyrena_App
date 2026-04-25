using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.APIReferences
{
    public class DocumentsExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            builder.AddApiReferencePages();
        }
    }
}

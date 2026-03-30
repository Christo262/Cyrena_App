using Cyrena.Extensa.Contracts;
using Cyrena.Options;

namespace Cyrena.Extensa.Models
{
    public abstract class Extension : IExtension
    {
        public virtual void BuildExtension(CyrenaBuilder builder)
        {
        }
    }
}

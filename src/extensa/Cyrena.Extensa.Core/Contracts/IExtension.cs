using Cyrena.Options;

namespace Cyrena.Extensa.Contracts
{
    /// <summary>
    /// The interface all extensions must implement
    /// </summary>
    public interface IExtension
    {        
        /// <summary>
        /// Called by the Extensa.Loader to add the extension to the application dependencies
        /// </summary>
        /// <param name="builder"><see cref="CyrenaBuilder"/></param>
        void BuildExtension(CyrenaBuilder builder);
    }
}

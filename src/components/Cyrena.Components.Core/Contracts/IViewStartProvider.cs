using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Provides optional <see cref="Models.ViewStart"/> for user to configure
    /// </summary>
    public interface IViewStartProvider
    {
        IEnumerable<ViewStart> Provide();
    }
}

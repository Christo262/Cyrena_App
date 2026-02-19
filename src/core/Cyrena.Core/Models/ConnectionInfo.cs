using Cyrena.Contracts;

namespace Cyrena.Models
{
    public record ConnectionInfo(string Id, string Name, string Source, string ModelId, IConnectionProvider Provider);
}

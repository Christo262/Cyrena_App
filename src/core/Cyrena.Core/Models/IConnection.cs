using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;

namespace Cyrena.Models
{
    public interface IConnection
    {
        Task HandleAsync(string input, AuthorRole role, IDeveloperContext context, CancellationToken ct = default);
    }

    public record ConnectionInfo(string Id, string Name, string Source, string ModelId, IConnectionProvider Provider);
}

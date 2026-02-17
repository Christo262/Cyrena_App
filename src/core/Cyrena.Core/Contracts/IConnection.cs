using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Connection to LLM service provider. Kernel locked
    /// </summary>
    public interface IConnection
    {
        Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default);
        Task HandleAsync(AuthorRole role, string input, Kernel kernel, CancellationToken ct = default, params AdditionalMessageContent[] items);
    }
}

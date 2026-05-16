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

        /// <summary>
        /// Used by a dedicated function invocation filter to inform when a function call starts to help suppress "thinking" messages and reduce context
        /// </summary>
        void FunctionCallStart();
    }
}

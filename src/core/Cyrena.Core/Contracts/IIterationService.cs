using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Contracts
{
    /// <summary>
    /// An iteration is from when a user sends a message until the model is complete and has replied. Kernel locked
    /// </summary>
    public interface IIterationService : IDisposable
    {
        bool Inferring { get; }
        /// <summary>
        /// <see cref="IConnection"/> invokes this
        /// </summary>
        void InferenceStart();
        /// <summary>
        /// <see cref="IConnection"/> invokes this
        /// </summary>
        void InferenceEnd();
        /// <summary>
        /// Listen for when iteration starts
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IDisposable OnIterationStart(Action<bool> callback);
        /// <summary>
        /// Listen for when iteration ends
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IDisposable OnIterationEnd(Action<bool> callback);
        void Iterate(AuthorRole role, string message, Kernel kernel);
        void Iterate(AuthorRole role, string message, Kernel kernel, params AdditionalMessageContent[] items);
    }
}

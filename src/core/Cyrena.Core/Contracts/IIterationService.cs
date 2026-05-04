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
        /// <summary>
        /// Use to keep current input user is typing in memory
        /// </summary>
        string? Input { get; set; }
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
        /// <summary>
        /// Queues the next iteration
        /// </summary>
        /// <param name="role"></param>
        /// <param name="kernel"></param>
        /// <param name="items"></param>
        void Iterate(AuthorRole role, Kernel kernel, params AdditionalMessageContent[]? items);
        /// <summary>
        /// Cancels the current iteration and pauses the input queue
        /// </summary>
        void Cancel();
        /// <summary>
        /// Pauses input queue
        /// </summary>
        void PauseQueue(bool by_ai = false);
        /// <summary>
        /// Resumes the input queue
        /// </summary>
        void ContinueQueue();
        /// <summary>
        /// true if queue is paused
        /// </summary>
        bool IsPaused { get; }
        int QueueCount { get; }
        bool IsPausedByAi { get; }
        IReadOnlyList<QueuedInput> Queued { get; }

        void CancelInput(string id);
    }
}

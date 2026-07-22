using Cyrena.Models;
using Cyrena.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Maintains chat history. Kernel locked
    /// </summary>
    public interface IChatMessageService : IDisposable
    {
        /// <summary>
        /// Contains user, assistant, system, tool call messages that are stored
        /// </summary>
        IReadOnlyList<ChatMessageContent> KernelHistory { get; }
        /// <summary>
        /// Contains only messages that need to be displayed to user
        /// </summary>
        IReadOnlyList<ChatMessageContent> DisplayHistory { get; }
        /// <summary>
        /// <see cref="ChatOptions"/>
        /// </summary>
        ChatOptions Options { get; }

        /// <summary>
        /// Callback for when a token is streamed
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IDisposable OnStreamToken(Action<string?> callback);
        IDisposable OnDisplayHistoryChanged(Action<ChatHistory> callback);
        IDisposable OnKernelHistoryChanged(Action<ChatHistory> callback);
        IDisposable OnHistoryLoaded(Action<ChatHistory> callback);
        /// <summary>
        /// Returns the kernel history for usage in <see cref="IConnection"/> 
        /// AFTER <see cref="IConversationHistoryTransformer.TransformPreIterationHistory(ChatHistory)"/> has been applied
        /// </summary>
        /// <returns><see cref="ChatHistory"/></returns>
        Task<ChatHistory> GetKernelHistory();

        /// <summary>
        /// Manually loads history
        /// </summary>
        /// <param name="kernelHistory"></param>
        /// <param name="displayHistory"></param>
        void LoadHistory(IEnumerable<ChatMessageContent> kernelHistory, IEnumerable<ChatMessageContent>? displayHistory);
        /// <summary>
        /// Loads history from database
        /// </summary>
        /// <returns></returns>
        Task LoadHistoryAsync();
        /// <summary>
        /// Adds a new message, auto checks if its kernel or ui only
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        Task AddMessage(ChatMessageContent content);       
        /// <summary>
        /// Removes the persisted chat messages and clears in memory messages
        /// </summary>
        /// <returns></returns>
        Task ClearHistoryAsync();

        void Stream(string? token);
    }
}

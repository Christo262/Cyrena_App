using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Used to modify the conversation history in anyway to ensure that context is short. Kernel Locked.
    /// </summary>
    public interface IConversationHistoryTransformer
    {
        /// <summary>
        /// Use to check post agent token streaming and apply changes to the persisted conversation history
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        Task ApplyPostStreamModification(ChatHistory history);

        /// <summary>
        /// Allows modification of the history to be sent to the Kernel before assistant gets the message
        /// </summary>
        /// <returns></returns>
        Task<ChatHistory> TransformPreIterationHistory(ChatHistory history);
    }

    public abstract class ConversationHistoryTransformer : IConversationHistoryTransformer
    {
        public virtual Task ApplyPostStreamModification(ChatHistory history)
        {
            return Task.CompletedTask;
        }

        public virtual Task<ChatHistory> TransformPreIterationHistory(ChatHistory history)
        {
            return Task.FromResult(history);
        }
    }
}

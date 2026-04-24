using Cyrena.Models;
using Cyrena.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Allows dynamic configuration of system prompts. Kernel Locked
    /// </summary>
    public interface IPromptManager
    {
        IReadOnlyList<Prompt> Prompts { get; }
        /// <summary>
        /// Adds a system prompt and returns the ID of the prompt
        /// </summary>
        /// <param name="order">Order in which the prompt should be loaded</param>
        /// <param name="content">Content of the prompt</param>
        /// <returns>Id</returns>
        string AddPrompt(int order, string content);
        /// <summary>
        /// Updates the content of a prompt
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="content">new content</param>
        void UpdatePrompt(string id, string content);
        /// <summary>
        /// Removes a prompt
        /// </summary>
        /// <param name="id"></param>
        void RemovePrompt(string id);

        /// <summary>
        /// Allows modification of the history that is returned in <see cref="IChatMessageService.GetKernelHistory"/>. 
        /// Leave null if no modification should take place. Helps to reduce context to only the required data the model needs and still retain
        /// history for the user
        /// </summary>
        Func<ChatHistory, ChatOptions, IEnumerable<ChatMessageContent>>? ModifyKernelHistoryFunc { get; set; }
    }
}

using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Configures behaviour surrounding the <see cref="Kernel"/>
    /// </summary>
    public interface IAssistantMode
    {
        public const string AssistantModeDefault = "assistant-default";
        /// <summary>
        /// Unique identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Configures the kernel
        /// </summary>
        /// <param name="config"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        Task ConfigureAsync(ChatConfiguration config, IKernelBuilder builder);
        /// <summary>
        /// Called by <see cref="IKernelController"/> when a chat is deleted
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Task DeleteAsync(ChatConfiguration config);
        /// <summary>
        /// Edits configuration
        /// </summary>
        /// <param name="config"><see cref="ChatConfiguration"/></param>
        /// <param name="services">scoped services of the current session</param>
        /// <returns></returns>
        Task EditAsync(ChatConfiguration config, IServiceProvider services);
    }
}

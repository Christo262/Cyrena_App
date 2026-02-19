using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Manages all <see cref="Microsoft.SemanticKernel.Kernel"/> instances and kills them when needed
    /// </summary>
    public interface IKernelController : IDisposable
    {
        /// <summary>
        /// Loads a conversation
        /// </summary>
        /// <param name="config"><see cref="ChatConfiguration"/></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="IAssistantMode"/> not found</exception>
        /// <exception cref="InvalidOperationException">If the Connection is not found</exception>
        /// <exception cref="Exception">If anything else fails</exception>
        Task<Kernel> LoadAsync(ChatConfiguration config);
        Task<Kernel> LoadAsync(string id);
        Task Delete(ChatConfiguration config);
        Task<Kernel> Create(ChatConfiguration config);
        Task UpdateAsync(ChatConfiguration config, bool reload = false);
        Kernel? GetKernel(string id);
        bool KernelActive(string id);
        void Unload(ChatConfiguration config);

        IDisposable OnChatDelete(Action<ChatConfiguration> cb);
        IDisposable OnChatCreate(Action<ChatConfiguration> cb);
        IDisposable OnChatUpdate(Action<ChatConfiguration> cb);
        IDisposable OnChatUnload(Action<ChatConfiguration> cb);

        //TODO Unload
    }
}

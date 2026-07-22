using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Used to determine component used for file attachments in a chat. Kernel Locked
    /// <see cref="FileAttacher"/> used to implement component
    /// </summary>
    public interface IFileAttacher : IComponent
    {
        EventCallback<KernelContent[]> OnItemsAdded { get; set; }
    }

    public abstract class FileAttacher : KernelComponentBase, IFileAttacher
    {
        [Parameter]
        public EventCallback<KernelContent[]> OnItemsAdded { get; set; }
    }
}

using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IDockingService
    {
        public record DockRequest(Type Component, string Title, Action OnClose);
        IDisposable OnDockRequest(Action<DockRequest> callback);
        void Dock<TKernelComponent>(string title, Action onClose)
            where TKernelComponent : KernelComponentBase;
    }
}

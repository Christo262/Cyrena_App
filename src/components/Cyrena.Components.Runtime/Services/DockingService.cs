using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Services
{
    internal class DockingService : IDockingService
    {
        private readonly DockingPipeline _pipe;
        public DockingService()
        {
            _pipe = new DockingPipeline();
        }

        public void Dock<TKernelComponent>(string title, Action onClose) where TKernelComponent : KernelComponentBase
        {
            var r = new IDockingService.DockRequest(typeof(TKernelComponent), title, onClose);
            _pipe.InvokeDockingRequest(r);
        }

        public IDisposable OnDockRequest(Action<IDockingService.DockRequest> callback) => _pipe.WatchRequest(callback);
    }

    internal class DockingPipeline : EventPipeline
    {
        public IDisposable WatchRequest(Action<IDockingService.DockRequest> cb) => this.ConfigurePipe("dock", cb);
        public void InvokeDockingRequest(IDockingService.DockRequest r) => this.InvokePipeline("dock", r);
    }
}

using Cyrena.Canvas.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Canvas.Services
{
    internal class CanvasStartupTask : IStartupTask
    {
        private readonly IKernelController _kernels;
        private readonly IStore<CanvasDocument> _store;
        public CanvasStartupTask(IKernelController kernels, IStore<CanvasDocument> store)
        {
            _kernels = kernels;
            _store = store;
        }

        public int Order => 10;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _kernels.OnChatDelete(async cfg =>
            {
                await _store.DeleteManyAsync(x => x.ConversationId == cfg.Id);
            });
            return Task.CompletedTask;
        }
    }
}

using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Runtime.Services
{
    internal class ChatConfigurationService : IChatConfigurationService
    {
        private readonly IKernelController _store;
        public ChatConfigurationService(IKernelController store, ChatConfiguration config)
        {
            Config = config;
            _store = store;
        }

        public ChatConfiguration Config { get; }

        public Task SaveConfigurationAsync()
        {
            return _store.UpdateAsync(Config);
        }
    }
}

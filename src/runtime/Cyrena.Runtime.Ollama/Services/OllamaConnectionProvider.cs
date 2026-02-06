using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Ollama.Models;
using Cyrena.Extensions;

namespace Cyrena.Runtime.Ollama.Services
{
    internal class OllamaConnectionProvider : IConnectionProvider
    {
        private readonly IStore<OllamaConnectionInfo> _store;
        public OllamaConnectionProvider(IStore<OllamaConnectionInfo> store)
        {
            _store = store;
        }

        public async Task<IConnection> CreateAsync(IKernelBuilder builder, string connectionId)
        {
            var connection = await _store.FindAsync(x => x.Id == connectionId);
            if (connection == null)
                throw new NullReferenceException($"Unable to find connection");
            var http = new HttpClient()
            {
                BaseAddress = new Uri(connection.Endpoint!),
                Timeout = TimeSpan.FromSeconds(60 * 3)
            };
            builder.AddOllamaChatCompletion(connection.ModelId!, http);
            builder.Services.AddSingleton<OllamaConnectionInfo>(connection);
            return new OllamaConnection();
        }

        public async Task<bool> HasConnectionAsync(string id)
        {
            var c = await _store.CountAsync(x => x.Id == id);
            return c > 0;
        }

        public async Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync()
        {
            var items = await _store.FindManyAsync(x => true);
            return items.Select(x => new ConnectionInfo(x.Id, x.Name, "Ollama", x.ModelId, this));
        }
    }
}

using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.OpenAI.Models;
using Cyrena.Runtime.OpenAI.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.OpenAI.Services
{
    internal class ConnectionProvider : IConnectionProvider
    {
        private readonly ISettingsService _settings;
        private readonly IStore<OpenAIModel> _store;
        public ConnectionProvider(ISettingsService settings, IStore<OpenAIModel> store)
        {
            _settings = settings;
            _store = store;
        }

        public async Task<ConnectionInfo> AttachAsync(IKernelBuilder builder, string connectionId)
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.ApiKey))
                throw new InvalidOperationException("OpenAI Configuration Incomplete");
            var model = await _store.FindAsync(x => x.Id == connectionId);
            if(model == null || string.IsNullOrEmpty(model.ModelId))
                throw new NullReferenceException($"Unable to find connection");
            var http = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            builder.AddOpenAIChatCompletion(model.ModelId, options.ApiKey, httpClient:http);
            builder.Services.AddSingleton<IConnection, OpenAIConnection>();
            builder.Services.AddSingleton(model);
            var info = new ConnectionInfo(model.Id, model.DisplayName ?? model.ModelId, "OpenAI", model.ModelId, this, model.SupportImages, model.SupportFiles);
            return info;
        }

        public async Task<bool> HasConnectionAsync(string id)
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.ApiKey))
                return false;
            var count = await _store.CountAsync(x => x.Id == id);
            return count > 0;
        }

        public async Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync()
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if(options == null || string.IsNullOrEmpty(options.ApiKey)) 
                return Enumerable.Empty<ConnectionInfo>();
            var models = await _store.FindManyAsync(x => !string.IsNullOrEmpty(x.ModelId));
            return models.Select(x => new ConnectionInfo(x.Id, x.DisplayName ?? x.ModelId!, "OpenAI", x.ModelId!, this, x.SupportImages, x.SupportFiles));
        }
    }
}

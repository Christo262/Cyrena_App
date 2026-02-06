using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.OpenAI.Options;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.OpenAI.Services
{
    internal class ConnectionProvider : IConnectionProvider
    {
        private readonly ISettingsService _settings;
        public ConnectionProvider(ISettingsService settings)
        {
            _settings = settings;
        }

        public Task<IConnection> CreateAsync(IKernelBuilder builder, string connectionId)
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ModelId))
                throw new InvalidOperationException("OpenAI Configuration Incomplete");
            if (connectionId != OpenAIOptions.Key)
                throw new InvalidOperationException("Invalid ConnectionId");

            builder.AddOpenAIChatCompletion(options.ModelId, options.ApiKey);
            var connection = new OpenAIConnection();
            return Task.FromResult<IConnection>(connection);
        }

        public Task<bool> HasConnectionAsync(string id)
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if (options == null || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ModelId))
                return Task.FromResult(false);
            return Task.FromResult(id == OpenAIOptions.Key);
        }

        public Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync()
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            if(options == null || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ModelId)) 
                return Task.FromResult(Enumerable.Empty<ConnectionInfo>());
            var info = new ConnectionInfo(OpenAIOptions.Key, "OpenAI", "OpenAI", options.ModelId, this);
            return Task.FromResult<IEnumerable<ConnectionInfo>>([info]);
        }
    }
}

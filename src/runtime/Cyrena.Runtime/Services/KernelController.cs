using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;

namespace Cyrena.Runtime.Services
{
    internal class KernelController : IKernelController
    {
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<string, Kernel> _instances;
        private readonly ControllerPipeline _pipe;
        private readonly IStore<ChatConfiguration> _store;
        public KernelController(IServiceProvider services, IStore<ChatConfiguration> store)
        {
            _instances = new ConcurrentDictionary<string, Kernel>();
            _services = services;
            _pipe = new ControllerPipeline();
            _store = store;
        }

        public IReadOnlyList<Kernel> ActiveKernels => _instances.Select(x => x.Value).ToList().AsReadOnly();

        public async Task<Kernel> LoadAsync(ChatConfiguration config)
        {
            if (_instances.ContainsKey(config.Id))
            {
                var ext = _instances.GetValueOrDefault(config.Id);
                return ext!;
            }

            try
            {
                _pipe.InvokeLoadStart(config);
                var modes = _services.GetServices<IAssistantMode>();
                var mode = modes.FirstOrDefault(x => x.Id == config.AssistantModeId);
                if (mode == null)
                    throw new NullReferenceException($"Unable to find assistant mode with id {config.AssistantModeId}");
                var connectionProviders = _services.GetServices<IConnectionProvider>();
                IConnectionProvider? connectionProvider = null;
                foreach (var provider in connectionProviders)
                {
                    if (await provider.HasConnectionAsync(config.ConnectionId))
                    {
                        connectionProvider = provider;
                        break;
                    }
                }
                if (connectionProvider == null)
                    throw new InvalidOperationException($"Unable to find connection provider for {config.ConnectionId}");
                IKernelBuilder builder = Kernel.CreateBuilder();
                builder.Services.AddLogging();
                builder.Services.AddSingleton(config);
                config.FileStoragePath = Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations", config.Id, "files");
                if (!Directory.Exists(config.FileStoragePath))
                    Directory.CreateDirectory(config.FileStoragePath);
                var info = await connectionProvider.AttachAsync(builder, config.ConnectionId);
                builder.Services.AddSingleton(info);
                builder.Services.AddSingleton<IKernelResolver>(new KernelResolver(config.Id, () => _instances[config.Id]));

                builder.Services.AddSingleton<IIterationService, IterationService>();

                var cyrenaKernelBuilder = new CyrenaKernelBuilder(config, builder);
                if (!Directory.Exists(Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations")))
                    Directory.CreateDirectory(Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations"));

                cyrenaKernelBuilder.AddIsolatedFilePersistence(Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations", config.Id), fs =>
                {
                    fs.AddSingletonStore<ChatMessageContentEntity>("messages");
                    fs.AddSingletonStore<FileAttachment>("file_metadata");
                });
                cyrenaKernelBuilder.Services.AddSingleton<IChatMessageService, ChatMessageService>();
                IPromptManager promptManager = new PromptManager();
                cyrenaKernelBuilder.AddFeatureOption<IPromptManager>(promptManager);
                cyrenaKernelBuilder.AddFeatureOption(info);
                await mode.ConfigureAsync(cyrenaKernelBuilder);

                IEnumerable<IAssistantPlugin> plugins = _services.GetServices<IAssistantPlugin>().Where(x => x.Modes.Length == 0 || x.Modes.Contains(mode.Id));
                if (!config.PluginIds.Any())
                    config.PluginIds = plugins.Select(x => x.Id).ToList();
                foreach (var plugin in plugins.OrderByDescending(x => x.Priority))
                {
                    if (config.PluginIds.Any(x => x == plugin.Id) || plugin.Required)
                        await plugin.LoadAsync(cyrenaKernelBuilder);
                }
                cyrenaKernelBuilder.Services.AddSingleton<IPromptManager>(promptManager);
                cyrenaKernelBuilder.Services.AddSingleton<IAutoFunctionInvocationFilter, ConnectionFunctionInformerFilter>();

                var kernel = builder.Build();
                if (!_instances.TryAdd(config.Id, kernel))
                {
                    DisposeKernel(kernel);
                    throw new Exception($"Unable to contain kernel instance");
                }
                var startups = kernel.Services.GetServices<IStartupTask>();
                foreach (var item in startups.OrderBy(x => x.Order))
                    await item.RunAsync();
                config.LastModified = DateTime.Now;
                await _store.UpdateAsync(config);
                _pipe.InvokeLoaded(config);
                return kernel;
            }
            catch(Exception ex)
            {
                _pipe.InvokeLoadError(ex);
                throw; //Continue normal handling
            }
        }

        public async Task<Kernel> LoadAsync(string id)
        {
            var config = await _store.FindAsync(x => x.Id == id);
            if (config == null)
                throw new Exception("Config not found");
            return await LoadAsync(config);
        }

        public async Task Delete(ChatConfiguration config)
        {
            var ext = await _store.FindAsync(x => x.Id == config.Id);
            if(ext != null)
                await _store.DeleteAsync(ext);
            if (_instances.TryRemove(config.Id, out var kernel))
            {
                _pipe.InvokeUnload(config);
                await Task.Delay(100); //Breather for unload pipe
                DisposeKernel(kernel);
            }
            var mode = _services.GetServices<IAssistantMode>().FirstOrDefault(x => x.Id == config.AssistantModeId);
            if (mode is not null)
                await mode.DeleteAsync(config);
            if(Directory.Exists(Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations", config.Id)))
                Directory.Delete(Path.Combine(CyrenaBuilder.AppDataDirectory, "conversations", config.Id), true);

            _pipe.InvokeDelete(config);
        }

        public async Task<Kernel> Create(ChatConfiguration config)
        {
            if (string.IsNullOrEmpty(config.Id))
                config.Id = Ulid.NewUlid().ToString();
            config.Created = DateTime.Now;
            await _store.AddAsync(config);
            _pipe.InvokeCreate(config);
            await Task.Delay(50); //Breather for pipe
            var model = await LoadAsync(config);
            return model;
        }

        public async Task UpdateAsync(ChatConfiguration config, bool reload = false)
        {
            await _store.UpdateAsync(config);
            if (!reload)
            {
                _pipe.InvokeUpdate(config);
                return;
            }

            if (_instances.TryRemove(config.Id, out var kernel))
            {
                //Make it look like a recreation
                _pipe.InvokeUnload(config);
                DisposeKernel(kernel);
                await Task.Delay(100);
                await LoadAsync(config);
                _pipe.InvokeLoaded(config);
            }
        }

        public void Unload(ChatConfiguration config)
        {
            if (_instances.TryRemove(config.Id, out var kernel))
            {
                _pipe.InvokeUnload(config);
                DisposeKernel(kernel);
            }
        }

        public IDisposable OnChatDelete(Action<ChatConfiguration> cb) => _pipe.WatchConfigDelete(cb);
        public IDisposable OnChatCreate(Action<ChatConfiguration> cb) => _pipe.WatchConfigCreate(cb);
        public IDisposable OnChatUpdate(Action<ChatConfiguration> cb) => _pipe.WatchConfigUpdate(cb);
        public IDisposable OnChatUnload(Action<ChatConfiguration> cb) => _pipe.WatchConfigUnload(cb);
        public IDisposable OnChatLoadStart(Action<ChatConfiguration> cb) => _pipe.WatchLoadStart(cb);
        public IDisposable OnChatLoaded(Action<ChatConfiguration> cb) => _pipe.WatchConfigLoaded(cb);
        public IDisposable OnChatLoadError(Action<Exception> cb) => _pipe.WatchConfigLoadError(cb);

        public void Dispose()
        {
            while (_instances.Count > 0)
            {
                if (_instances.TryRemove(_instances.First().Key, out var kernel))
                    DisposeKernel(kernel);
            }
        }

        private void DisposeKernel(Kernel kernel)
        {
            //semantic kernel does not expose dispose at this time, so we need to hack it.
            switch (kernel.Services)
            {
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        public Kernel? GetKernel(string id)
        {
            if (_instances.TryGetValue(id, out var kernel))
                return kernel;
            return null;
        }

        public bool KernelActive(string id)
        {
            return _instances.ContainsKey(id);
        }

        internal class ControllerPipeline : EventPipeline
        {
            public IDisposable WatchConfigCreate(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_create", callback);
            public IDisposable WatchConfigDelete(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_delete", callback);
            public IDisposable WatchConfigUpdate(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_update", callback);
            public IDisposable WatchConfigUnload(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_unload", callback);
            public IDisposable WatchLoadStart(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_load_start", callback);
            public IDisposable WatchConfigLoaded(Action<ChatConfiguration> callback) => this.ConfigurePipe("k_loaded", callback);
            public IDisposable WatchConfigLoadError(Action<Exception> callback) => this.ConfigurePipe("k_load_error", callback);

            public void InvokeCreate(ChatConfiguration config) => InvokePipeline("k_create", config);
            public void InvokeDelete(ChatConfiguration config) => InvokePipeline("k_delete", config);
            public void InvokeUpdate(ChatConfiguration config) => InvokePipeline("k_update", config);
            public void InvokeUnload(ChatConfiguration config) => InvokePipeline("k_unload", config);
            public void InvokeLoadStart(ChatConfiguration config) => InvokePipeline("k_load_start", config);
            public void InvokeLoaded(ChatConfiguration config) => InvokePipeline("k_loaded", config);
            public void InvokeLoadError(Exception ex) => InvokePipeline("k_load_error", ex);
        }
    }
}

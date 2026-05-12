using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    internal class ChatMessageService : IChatMessageService
    {
        private readonly ChatMessagePipeline _pipeline;
        private readonly ChatOptions _options;
        private readonly IStore<ChatMessage> _store;
        private readonly ChatConfiguration _config;
        private readonly IPromptManager _prompts;
        private readonly IIterationService _its;
        private readonly IServiceProvider _services;

        private readonly ChatHistory _kernel;
        private readonly ChatHistory _display;
        public ChatMessageService(IOptions<ChatOptions> options, ChatConfiguration config, IStore<ChatMessage> store, IPromptManager prompts, IIterationService its, IServiceProvider services)
        {
            _options = options.Value;
            _pipeline = new ChatMessagePipeline();
            _store = store;
            _config = config;
            _prompts = prompts;

            _kernel = new ChatHistory();
            _display = new ChatHistory();
            _its = its;
            _services = services;
        }

        public ChatOptions Options => _options;
        public IReadOnlyList<ChatMessageContent> KernelHistory => _kernel;
        public IReadOnlyList<ChatMessageContent> DisplayHistory => _display;
        public async Task<ChatHistory> GetKernelHistory()
        {
            var history = new ChatHistory(_kernel.Where(x => x.Role != _options.System));
            var transformers = _services.GetServices<IConversationHistoryTransformer>();
            foreach (var interceptor in transformers)
                history = await interceptor.TransformPreIterationHistory(history);

            var final = new ChatHistory();
            foreach (var item in _prompts.Prompts.OrderBy(x => x.Order))
                final.AddSystemMessage(item.Content);
            final.AddRange(history);

            return final;
        }

        public IDisposable OnStreamToken(Action<string?> callback) => _pipeline.WatchStreamToken(callback);
        public IDisposable OnDisplayHistoryChanged(Action<ChatHistory> callback) => _pipeline.WatchDisplayHistoryUpdated(callback);
        public IDisposable OnKernelHistoryChanged(Action<ChatHistory> callback) => _pipeline.WatchKernelHistoryUpdated(callback);
        public IDisposable OnHistoryLoaded(Action<ChatHistory> callback) => _pipeline.WatchHistoryLoaded(callback);

        public void LoadHistory(IEnumerable<ChatMessageContent> kernelHistory, IEnumerable<ChatMessageContent>? displayHistory)
        {
            _kernel.Clear();
            _kernel.AddRange(kernelHistory);
            _display.Clear();
            if (displayHistory == null)
                _display.AddRange(_kernel.Where(x => _options.IsDisplayContent(x)));
            else _display.AddRange(displayHistory);
            _pipeline.InvokeHistoryLoaded(_kernel);
        }

        public async Task LoadHistoryAsync()
        {
            var data = await _store.FindManyAsync(x => x.ConversationId == _config.Id, new OrderBy<ChatMessage>(x => x.Date, SortDirection.Ascending));
            var k_history = data.Select(x => new ChatMessageContent(new AuthorRole(x.Label), x.Content));
            var d_history = data.Select(x => x.ToDisplayMessageContent());
            d_history = d_history.Where(x => _options.IsDisplayContent(x));
            LoadHistory(k_history, d_history);
        }

        public async Task AddMessage(ChatMessageContent content)
        {
            if(_options.IsKernelContent(content))
            {
                _kernel.Add(content);
                _pipeline.InvokeKernelHistoryUpdated(_kernel);
                if (_options.MessagePersistRoles.Contains(content.Role))
                    await _store.AddAsync(new ChatMessage(content, _config.Id, _its.IterationId));
            }

            if(_options.IsDisplayContent(content))
            {
                _display.Add(content);
                _pipeline.InvokeDisplayHistoryUpdated(_display);
            }
        }

        public async Task AddMessage(AuthorRole role, string? content)
        {
            var model = new ChatMessageContent(role, content);
            await AddMessage(model);
        }

        public async Task AddMessage(AuthorRole role, string? input, params AdditionalMessageContent[] items)
        {
            var content = new ChatMessageContent(role, input);
            if (_options.IsKernelContent(content))
            {
                _kernel.Add(content);
                _pipeline.InvokeKernelHistoryUpdated(_kernel);
                if (items.Any())
                    foreach (var item in items)
                        content.Items.Add(item.Item);
                if(_options.MessagePersistRoles.Contains(content.Role))
                    await _store.AddAsync(new ChatMessage(content, _config.Id, _its.IterationId, items));
            }

            if (_options.IsDisplayContent(content))
            {
                if (items.Any())
                {
                    var cmpn = new ChatMessageContent(role, input);
                    foreach (var item in items)
                        cmpn.Items.Add(new InfoMessageContentItem(item.Name));
                    _display.Add(cmpn);
                    _pipeline.InvokeDisplayHistoryUpdated(_display);
                }
                else
                {
                    _display.Add(content);
                    _pipeline.InvokeDisplayHistoryUpdated(_display);
                }
            }
        }

        public async Task ClearHistoryAsync()
        {
            var count = await _store.DeleteManyAsync(x => x.ConversationId == _config.Id);
            _kernel.Clear();
            _display.Clear();
            _pipeline.InvokeKernelHistoryUpdated(_kernel);
            _pipeline.InvokeDisplayHistoryUpdated(_display);
        }

        public void Dispose()
        {
            _pipeline.Dispose();
        }

        public void Stream(string? token)
        {
            _pipeline.InvokeStreamToken(token);
        }

        internal class ChatMessagePipeline : EventPipeline
        {
            public IDisposable WatchDisplayHistoryUpdated(Action<ChatHistory> callback)
            {
                return this.ConfigurePipe<ChatHistory>("display_history", callback);
            }

            public IDisposable WatchKernelHistoryUpdated(Action<ChatHistory> callback)
            {
                return this.ConfigurePipe<ChatHistory>("kernel_history", callback);
            }

            public IDisposable WatchHistoryLoaded(Action<ChatHistory> callback)
            {
                return this.ConfigurePipe<ChatHistory>("history_loaded", callback);
            }

            public IDisposable WatchStreamToken(Action<string?> callback)
            {
                return this.ConfigurePipe<string?>("on_stream", callback);
            }

            public void InvokeDisplayHistoryUpdated(ChatHistory history)
            {
                this.InvokePipeline("display_history", history);
            }

            public void InvokeKernelHistoryUpdated(ChatHistory history)
            {
                this.InvokePipeline("kernel_history", history);
            }

            public void InvokeHistoryLoaded(ChatHistory history)
            {
                this.InvokePipeline("history_loaded", history);
            }

            public void InvokeStreamToken(string? token)
            {
                this.InvokePipeline("on_stream", token);
            }
        }
    }
}

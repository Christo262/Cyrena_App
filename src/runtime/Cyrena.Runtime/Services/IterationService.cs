using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    internal class IterationService : BackgroundService, IIterationService
    {
        private readonly IterationPipeline _pipeline;
        private readonly InputQueue _queue;
        private readonly CancellationTokenSource _worker_token;
        /// <summary>
        /// Will be set on Iterate()
        /// </summary>
        private Kernel _kernel { get; set; } = default!;
        private Ulid? _iteration_id { get; set; }
        public IterationService()
        {
            _pipeline = new IterationPipeline();
            _queue = new InputQueue();
            _worker_token = new CancellationTokenSource();
        }

        public string? Input { get; set; }
        public bool Inferring { get; private set; }

        public void InferenceEnd()
        {
            lock (_queue)
            {
                Inferring = false;
                _pipeline.InvokeIteration(Inferring);
                _iteration_id = null;
            }
        }

        public void InferenceStart()
        {
            lock (_queue)
            {
                Inferring = true;
                _pipeline.InvokeIteration(Inferring);
                _iteration_id = Ulid.NewUlid();
            }
        }

        public string? IterationId => _iteration_id?.ToString();

        public IDisposable OnIterationStart(Action<bool> callback)
        {
            return _pipeline.WatchIterationStart(callback);
        }

        public IDisposable OnIterationEnd(Action<bool> callback)
        {
            return _pipeline.WatchIterationEnd(callback);
        }

        public override void Dispose()
        {
            _pipeline.Dispose();
            _worker_token.Cancel();
            this.StopAsync(_worker_token.Token);
            _worker_token.Dispose();
        }

        private CancellationTokenSource? _token { get; set; }

        public void Iterate(AuthorRole role, Kernel kernel, params AdditionalMessageContent[]? items)
        {
            if(_kernel == null)
            {
                _kernel = kernel;
                this.StartAsync(_worker_token.Token).Wait();
            }
            if (string.IsNullOrEmpty(Input))
                return;
            if(IsPausedByAi)
            {
                _queue.EnqueueAt(0, role, Input.Trim(), items);
                ContinueQueue();
            }
            else
                _queue.Enqueue(role, Input.Trim(), items);
            Input = null;
        }

        public void Cancel()
        {
            if (_token == null || _token.IsCancellationRequested) return;
            _token.Cancel();
            _queue.Pause();
        }

        public bool IsPaused => _queue.Paused;
        public int QueueCount => _queue.Count;
        public IReadOnlyList<QueuedInput> Queued => _queue.GetSnapshot();
        public bool IsPausedByAi { get; private set; }

        public void PauseQueue(bool by_ai = false)
        {
            IsPausedByAi = by_ai;
            _queue.Pause();
        }

        public void ContinueQueue()
        {
            IsPausedByAi = false;
            _queue.Continue();
        }

        public void CancelInput(string id)
        {
            _queue.Pause();
            _queue.Remove(id);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(_queue.Paused)
                {
                    await Task.Delay(100);
                    continue;
                }
                if (!Inferring)
                {
                    var q = _queue.Dequeue();
                    if (q == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }
                    var input = q.Content;
                    var items = q.Items.Count == 0 ? null : q.Items.ToArray();
                    var role = q.Role;
                    try
                    {
                        _token?.Dispose();
                        _token = new CancellationTokenSource();
                        IConnection connection = _kernel.Services.GetRequiredService<IConnection>();
                        if (items == null)
                            await connection.HandleAsync(role, input, _kernel, _token.Token);
                        else
                            await connection.HandleAsync(role, input, _kernel, _token.Token, items);
                    }
                    catch (TaskCanceledException)
                    {
                        InferenceEnd();
                    }
                    catch (Exception ex)
                    {
                        await _kernel.GetRequiredService<IChatMessageService>().LogError(ex.Message);
                        InferenceEnd();
                    }
                }
                await Task.Delay(2000);
            }
        }

        internal class IterationPipeline : EventPipeline
        {
            public IDisposable WatchIterationStart(Action<bool> callback)
            {
                return this.ConfigurePipe("iteration_start", callback);
            }

            public IDisposable WatchIterationEnd(Action<bool> callback)
            {
                return this.ConfigurePipe("iteration_end", callback);
            }

            public void InvokeIteration(bool e)
            {
                if (e)
                    this.InvokePipeline("iteration_start", e);
                else
                    this.InvokePipeline("iteration_end", e);
            }
        }
    }
}

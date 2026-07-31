using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Runtime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data;

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
        private readonly IKernelResolver _kernel;
        private Ulid? _iteration_id { get; set; }
        public IterationService(IKernelResolver kernel)
        {
            _pipeline = new IterationPipeline();
            _queue = new InputQueue();
            _worker_token = new CancellationTokenSource();
            _kernel = kernel;
        }

        public ChatMessageContent? Input { get; set; }
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

        public Ulid? IterationId => _iteration_id;

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
        private bool _queue_started { get; set; }
        public void Iterate()
        {
            if(!_queue_started)
            {
                this.StartAsync(_worker_token.Token).Wait();
                _queue_started = true;
            }
            if (Input == null)
                return;
            if(IsPausedByAi)
            {
                _queue.EnqueueAt(0, Input);
                ContinueQueue();
            }
            else
                _queue.Enqueue(Input);
            Input = new ChatMessageContent(Input.Role, "");
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
                    var kernel = _kernel.Resolve();
                    try
                    {
                        _token?.Dispose();
                        _token = new CancellationTokenSource();
                        IConnection connection = kernel.Services.GetRequiredService<IConnection>();
                        await connection.HandleAsync(q.Message, _token.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        InferenceEnd();
                    }
                    catch (Exception ex)
                    {
                        var chat = kernel.GetRequiredService<IChatMessageService>();
                        await chat.LogError(ex.Message);
                        
                        InferenceEnd();
                    }
                }
                await Task.Delay(1000);
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

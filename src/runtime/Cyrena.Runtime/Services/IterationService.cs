using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    internal class IterationService : IIterationService
    {
        private readonly IterationPipeline _pipeline;
        public IterationService()
        {
            _pipeline = new IterationPipeline();
        }

        public bool Inferring { get; private set; }

        public void InferenceEnd()
        {
            Inferring = false;
            _pipeline.InvokeIteration(Inferring);
        }

        public void InferenceStart()
        {
            Inferring = true;
            _pipeline.InvokeIteration(Inferring);
        }

        public IDisposable OnIterationStart(Action<bool> callback)
        {
            return _pipeline.WatchIterationStart(callback);
        }

        public IDisposable OnIterationEnd(Action<bool> callback)
        {
            return _pipeline.WatchIterationEnd(callback);
        }

        public void Dispose()
        {
            _pipeline.Dispose();
        }

        private Task? _handle { get; set; }
        private CancellationTokenSource? _token { get; set; }
        public void Iterate(AuthorRole role, string message, Kernel kernel)
        {
            if (_handle != null)
            {
                if (_handle.IsCompleted == false)
                    return;
                _handle.Dispose();
                _handle = null;
            }
            if (_token != null)
            {
                _token.Cancel();
                _token.Dispose();
            }
            _token = new CancellationTokenSource();
            _handle = Task.Run(async () =>
            {
                try
                {
                    IConnection connection = kernel.Services.GetRequiredService<IConnection>();
                    await connection.HandleAsync(role, message, kernel, _token.Token);
                }
                catch (Exception ex)
                {
                    await kernel.GetRequiredService<IChatMessageService>().LogError(ex.Message);
                }
            }, _token.Token);
        }

        public void Iterate(AuthorRole role, string message, Kernel kernel, params AdditionalMessageContent[] items)
        {
            if (_handle != null)
            {
                if (_handle.IsCompleted == false)
                    return;
                _handle.Dispose();
                _handle = null;
            }
            if (_token != null)
            {
                _token.Cancel();
                _token.Dispose();
            }
            _token = new CancellationTokenSource();
            _handle = Task.Run(async () =>
            {
                try
                {
                    IConnection connection = kernel.Services.GetRequiredService<IConnection>();
                    await connection.HandleAsync(role, message, kernel, _token.Token, items);
                }
                catch (Exception ex)
                {
                    await kernel.GetRequiredService<IChatMessageService>().LogError(ex.Message);
                }
            }, _token.Token);
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

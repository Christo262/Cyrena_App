using System.Collections.Concurrent;

namespace Cyrena.Models
{
    public interface IEventPipe : IDisposable
    {
        void Invoke();
        void Invoke(object obj);
        bool IsDisposed { get; }
    }

    public class EventPipe : IEventPipe
    {
        private readonly Action _action;
        public EventPipe(Action action)
        {
            _action = action;
        }

        private bool _disposed { get; set; }
        public void Dispose() => _disposed = true;

        public void Invoke() => _action();

        public void Invoke(object obj) => Invoke();

        public bool IsDisposed => _disposed;
    }

    public class EventPipe<T> : IEventPipe
    {
        private readonly Action<T> _action;
        public EventPipe(Action<T> action)
        {
            _action = action;
        }

        private bool _disposed { get; set; }
        public void Dispose() => _disposed = true;

        public void Invoke() { throw new NotImplementedException(); }

        public void Invoke(object obj)
        {
            if (obj is T t)
                _action(t);
        }

        public bool IsDisposed => _disposed;
    }

    public abstract class EventPipeline : IDisposable
    {
        private readonly ConcurrentDictionary<string, List<IEventPipe>> _pipes;
        private readonly object _lock = new object();
        protected EventPipeline()
        {
            _pipes = new ConcurrentDictionary<string, List<IEventPipe>>();
        }

        protected void InvokePipeline(string key)
        {
            if (_pipes.ContainsKey(key))
            {
                var pipes = new List<IEventPipe>(_pipes[key]);
                foreach (var pipe in pipes)
                    if (!pipe.IsDisposed)
                        try
                        {
                            pipe.Invoke();
                        }
                        catch
                        {
                            pipe.Dispose();
                        }
                pipes.RemoveAll(p => p.IsDisposed);
            }
        }

        protected void InvokePipeline<T>(string key, T value)
        {
            if (_pipes.ContainsKey(key))
            {
                var pipes = new List<IEventPipe>(_pipes[key]);
                foreach (var pipe in pipes)
                    if (!pipe.IsDisposed)
                        try
                        {
                            pipe.Invoke(value!);
                        }
                        catch
                        {
                            pipe.Dispose();
                        }
                pipes.RemoveAll(p => p.IsDisposed);
            }
        }

        protected IDisposable ConfigurePipe(string key, Action cb)
        {
            var pipe = new EventPipe(cb);
            List<IEventPipe> pipes;
            if (_pipes.ContainsKey(key))
                pipes = _pipes[key];
            else
            {
                pipes = new List<IEventPipe>();
                _pipes.TryAdd(key, pipes);
            }
            pipes.Add(pipe);
            return pipe;
        }

        protected IDisposable ConfigurePipe<T>(string key, Action<T> cb)
        {
            var pipe = new EventPipe<T>(cb);
            List<IEventPipe> pipes;
            if (_pipes.ContainsKey(key))
                pipes = _pipes[key];
            else
            {
                pipes = new List<IEventPipe>();
                _pipes.TryAdd(key, pipes);
            }
            pipes.Add(pipe);
            return pipe;
        }

        public void Dispose()
        {
            foreach (var pipe in _pipes)
            {
                pipe.Value.ForEach(e => e.Dispose());
            }
        }
    }
}

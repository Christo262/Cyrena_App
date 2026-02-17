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

        public void Invoke(object obj)
        {
            throw new NotImplementedException();
        }

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
        private readonly Dictionary<string, List<IEventPipe>> _pipes;
        protected EventPipeline()
        {
            _pipes = new Dictionary<string, List<IEventPipe>>();
        }

        protected void InvokePipeline(string key)
        {
            if (_pipes.ContainsKey(key))
            {
                var pipes = _pipes[key];
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
                var dsp = _pipes[key].Where(x => x.IsDisposed).ToList();
                foreach (var pipe in dsp)
                    pipes.Remove(pipe);
            }
        }

        protected void InvokePipeline<T>(string key, T value)
        {
            if (_pipes.ContainsKey(key))
            {
                var pipes = _pipes[key];
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
                var dsp = _pipes[key].Where(x => x.IsDisposed).ToList();
                foreach (var pipe in dsp)
                    pipes.Remove(pipe);
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
                _pipes.Add(key, pipes);
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
                _pipes.Add(key, pipes);
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Options
{
    public sealed class CyrenaBuilder
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<string, CyrenaOption> _options;
        private readonly List<Action<CyrenaBuilder>> _buildActions;
        internal CyrenaBuilder(IServiceCollection services)
        {
            _services = services;
            _options = new Dictionary<string, CyrenaOption>();
            _buildActions = new List<Action<CyrenaBuilder>>();
        }

        public IServiceCollection Services => _services;

        public void AddOption(string key, object value)
        {
            _options[key] = new CyrenaOption(value.GetType(), value);
        }

        public void AddOption<TType>(string key, object value)
        {
            _options[key] = new CyrenaOption(typeof(TType), value);
        }

        public object? GetOption(string key)
        {
            if (!_options.ContainsKey(key))
                return null;
            return _options[key].Value;
        }

        public T GetOption<T>(string key)
        {
            if (!_options.ContainsKey(key))
                throw new NullReferenceException($"Unable to find {key}");
            var obj = _options[key].Value;
            if (obj is T t)
                return t;
            throw new InvalidCastException($"Unable to cast {obj.GetType()} to {typeof(T)}");
        }

        public T GetOption<T>()
        {
            foreach (var item in _options.Values)
            {
                if (item.Value is T t) return t;
                if (typeof(T).IsAssignableFrom(item.ServiceType)) return (T)item.Value;
            }
            throw new NullReferenceException($"Unable to find {typeof(T)}");
        }

        public static CyrenaBuilder Create(IServiceCollection services)
        {
            return new CyrenaBuilder(services);
        }

        public void AddBuildAction(Action<CyrenaBuilder> action)
        {
            _buildActions.Add(action);
        }

        public void Build()
        {
            foreach (var item in _buildActions)
                item.Invoke(this);
        }
    }

    internal record CyrenaOption(Type ServiceType, object Value);
}

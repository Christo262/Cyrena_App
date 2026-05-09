using Cyrena.Synthesis.Contracts;
using System.Text.Json;

namespace Cyrena.Synthesis.Models
{
    internal class CapabilityArgs : ICapabilityArgs
    {
        private readonly Dictionary<string, string> _arguments;

        public IReadOnlyList<string> Names => _arguments.Keys.ToList();
        public int Count => _arguments.Count;

        public CapabilityArgs(IEnumerable<KeyValuePair<string, string>> arguments)
        {
            _arguments = new Dictionary<string, string>(arguments ?? Enumerable.Empty<KeyValuePair<string, string>>(), StringComparer.OrdinalIgnoreCase);
        }

        public CapabilityArgs(IDictionary<string, string> arguments)
        {
            _arguments = new Dictionary<string, string>(arguments ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
        }

        public string GetString(string name)
        {
            if (_arguments.TryGetValue(name, out var value))
            {
                return value ?? string.Empty;
            }
            return string.Empty;
        }

        public int GetInt32(string name)
        {
            if (_arguments.TryGetValue(name, out var value) && int.TryParse(value, out var result))
            {
                return result;
            }
            return 0;
        }

        public bool GetBoolean(string name)
        {
            if (_arguments.TryGetValue(name, out var value))
            {
                if (bool.TryParse(value, out var result))
                {
                    return result;
                }
                // Support common boolean string representations
                var lowered = value.Trim().ToLowerInvariant();
                if (lowered == "1" || lowered == "yes" || lowered == "true" || lowered == "on")
                {
                    return true;
                }
                if (lowered == "0" || lowered == "no" || lowered == "false" || lowered == "off")
                {
                    return false;
                }
            }
            return false;
        }

        public double GetDouble(string name)
        {
            if (_arguments.TryGetValue(name, out var value) && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0.0;
        }

        public T GetJson<T>(string name)
        {
            if (_arguments.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(value) ?? default!;
                }
                catch (JsonException)
                {
                    return default!;
                }
            }
            return default!;
        }

        public bool Has(string name)
        {
            return _arguments.ContainsKey(name);
        }

        public string? GetRaw(string name)
        {
            if (_arguments.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }
}

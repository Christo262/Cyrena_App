using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using System.Text.Json;

namespace Cyrena.Synthesis.Services
{
    internal sealed class CapabilityResultWriter : ICapabilityResultWriter
    {
        private readonly CapabilityResultBag _bag;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly int _maxJsonBytes;

        public CapabilityResultWriter()
        {
            _bag = new CapabilityResultBag();
            _jsonOptions = new JsonSerializerOptions();
            _maxJsonBytes = 64_800;
        }

        public CapabilityResultBag ResultBag => _bag;

        public void Text(string key, string value)
        {
            Set(key, "text", value);
        }

        public void Number(string key, double value)
        {
            Set(key, "number", value);
        }

        public void Boolean(string key, bool value)
        {
            Set(key, "boolean", value);
        }

        public void Json<T>(string key, T value)
        {
            Set(key, "json", value);
        }

        public void Error(string code, string message)
        {
            Set("error", "error", new CapabilityErrorResult(code, message));
        }

        private void Set<T>(string key, string type, T value)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

            if (bytes.Length > _maxJsonBytes)
                throw new InvalidOperationException($"Result '{key}' exceeded maximum size.");

            using var doc = JsonDocument.Parse(bytes);

            _bag.Set(key, new CapabilityResultValue
            {
                Type = type,
                Value = doc.RootElement.Clone()
            });
        }
    }
}

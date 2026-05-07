using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Cyrena.Synthesis.Models
{
    public sealed class CapabilityResultBag
    {
        private readonly Dictionary<string, CapabilityResultValue> _values = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, CapabilityResultValue> Values => _values;

        public void Set(string key, CapabilityResultValue value)
        {
            ValidateKey(key);
            _values[key] = value;
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Result key is required.");

            if (key.Length > 64)
                throw new ArgumentException("Result key is too long.");

            if (!key.All(c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.'))
                throw new ArgumentException("Result key contains invalid characters.");
        }
    }

    public sealed class CapabilityResultValue
    {
        public required string Type { get; init; }
        public required JsonElement Value { get; init; }
    }
}

using Cyrena.Models;

namespace Cyrena.LTM.Models
{
    /// <summary>
    /// A structured fact stored within a memory entry.
    /// Facts are typed key-value pairs that capture specific details the AI wants to remember.
    /// The <see cref="FactType"/> is a free-form string set by the AI — not an enum —
    /// so the AI can invent and evolve its own fact types organically (e.g. "preference", "event", "relationship", "skill", "goal").
    /// </summary>
    public class MemoryFact : Entity
    {
        public MemoryFact()
        {
            Id = Ulid.NewUlid().ToString();
            _properties = new Dictionary<string, string?>();
        }

        internal Dictionary<string, string?> _properties { get; set; }

        /// <summary>
        /// Gets or sets a property value by key.
        /// Setting null or empty removes the key.
        /// </summary>
        public string? this[string key]
        {
            get
            {
                if (!_properties.ContainsKey(key))
                    return null;
                return _properties[key];
            }
            set
            {
                if (string.IsNullOrEmpty(value) && _properties.ContainsKey(key))
                    _properties.Remove(key);
                else
                    _properties[key] = value;
            }
        }

        /// <summary>
        /// The fact type — a free-form label the AI assigns to categorize this fact.
        /// Examples: "preference", "event", "relationship", "skill", "goal", "habit", "fact".
        /// The AI can invent new types at any time; there is no fixed enum.
        /// </summary>
        public string FactType { get; set; } = string.Empty;

        /// <summary>
        /// All property keys currently set on this fact.
        /// </summary>
        public IEnumerable<string> Keys => _properties.Keys;

        /// <summary>
        /// All property values (non-null) currently set on this fact.
        /// </summary>
        public IEnumerable<string?> Values => _properties.Values;

        /// <summary>
        /// Returns a human-readable summary of this fact for AI consumption.
        /// </summary>
        public override string ToString()
        {
            var props = string.Join(", ", _properties.Select(kv => $"{kv.Key}={kv.Value}"));
            return $"[{FactType}] {props}";
        }
    }
}

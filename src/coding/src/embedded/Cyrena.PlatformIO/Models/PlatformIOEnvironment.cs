using Cyrena.Models;

namespace Cyrena.PlatformIO.Models
{
    public sealed class PlatformIOEnvironment
    {
        private readonly Dictionary<string, string?> _properties = new();

        public string Name { get; set; } = default!;

        public IReadOnlyDictionary<string, string?> Properties => _properties;

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
                _properties[key] = value;
            }
        }

        public string? Framework
        {
            get => this["framework"];
            set => this["framework"] = value;
        }

        public static List<PlatformIOEnvironment> Parse(string path)
        {
            var globals = new Dictionary<string, string?>();
            var envs = new List<PlatformIOEnvironment>();

            PlatformIOEnvironment? currentEnv = null;
            bool inGlobalEnv = false;

            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                // section header
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var section = line[1..^1];

                    if (section == "env")
                    {
                        inGlobalEnv = true;
                        currentEnv = null;
                        continue;
                    }

                    if (section.StartsWith("env:"))
                    {
                        inGlobalEnv = false;

                        currentEnv = new PlatformIOEnvironment
                        {
                            Name = section // keep full [env:uno] label
                        };

                        // inherit globals immediately
                        foreach (var kv in globals)
                            currentEnv[kv.Key] = kv.Value;

                        envs.Add(currentEnv);
                        continue;
                    }

                    // ignore other sections
                    inGlobalEnv = false;
                    currentEnv = null;
                    continue;
                }

                // key=value
                var idx = line.IndexOf('=');
                if (idx <= 0)
                    continue;

                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();

                if (inGlobalEnv)
                {
                    globals[key] = value;
                }
                else
                {
                    currentEnv?[key] = value;
                }
            }

            return envs;
        }
    }
}

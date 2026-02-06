using Newtonsoft.Json;
using Cyrena.Contracts;

namespace Cyrena.Runtime.Services
{
    internal class SettingsService : ISettingsService
    {
        private readonly string _dir;
        private readonly object _lock;
        public SettingsService(string dir)
        {
            _dir = dir;
            _lock = new object();
            EnsureDirectory();
        }

        public T? Read<T>(string key) where T : class
        {
            lock (_lock)
            {
                var path = Path.Combine(_dir, $"{key}.settings");
                if (!File.Exists(path))
                    return null;
                try
                {
                    var json = File.ReadAllText(path);
                    var t = JsonConvert.DeserializeObject<T>(json);
                    return t;
                }
                catch { return null; }
            }
        }

        public void Save<T>(string key, T value) where T : class
        {
            lock (_lock)
            {
                var path = Path.Combine(_dir, $"{key}.settings");
                var json = JsonConvert.SerializeObject(value);
                File.WriteAllText(path, json);
            }
        }

        private void EnsureDirectory()
        {
            var dir = Path.Combine(_dir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}

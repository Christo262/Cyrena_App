using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Cyrena.Components.Shared
{
    public partial class Mascot
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        private List<MascotInfo> _mascots = [];

        private MascotInfo? _current = default!;

        protected override void OnInitialized()
        {
            try
            {
                var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
                var dir = Path.Combine("./wwwroot", "_content", "Cyrena", "mascots");
                if (!Directory.Exists(dir)) return;
                var json = File.ReadAllText(Path.Combine(dir, "mascots.json"));
                _mascots = JsonSerializer.Deserialize<List<MascotInfo>>(json) ?? new List<MascotInfo>();
                if (string.IsNullOrEmpty(customs.Mascot))
                    _current = _mascots.FirstOrDefault();
                else
                    _current = _mascots.FirstOrDefault(x => x.File == customs.Mascot);
            }
            catch
            {
                _current = null;
            }
        }

        private string MascotPath()
        {
            if (_current == null)
                return "images/rene_f.png";
            var dir = Path.Combine("_content", "Cyrena", "mascots", _current.File!);
            return dir;
        }

        private void Previous()
        {
            var cidx =_current == null ? 0: _mascots.FindIndex(x => x.File == _current.File);

            if (cidx <= 0)
                _current = _mascots.Last();
            else
                _current = _mascots[cidx - 1];
            var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
            customs.Mascot = _current.File;
            _settings.Save(Customization.Key, customs);

            StateHasChanged();
        }

        private void Next()
        {
            var cidx = _current == null ? 0 : _mascots.FindIndex(x => x.File == _current.File);

            if (cidx >= _mascots.Count - 1)
                _current = _mascots.First();
            else
                _current = _mascots[cidx + 1];

            var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
            customs.Mascot = _current.File;
            _settings.Save(Customization.Key, customs);

            StateHasChanged();
        }
    }

    internal class MascotInfo
    {
        public string? Name { get; set; }
        public string? File { get; set; }
        public int Height { get; set; } = 400;
    }
}

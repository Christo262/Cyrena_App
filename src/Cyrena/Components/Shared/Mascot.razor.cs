using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MudBlazor.Icons;

#if ANDROID
using Microsoft.Maui.Storage;
#endif

namespace Cyrena.Components.Shared
{
    public partial class Mascot
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        private List<MascotInfo> _mascots = [];

        private MascotInfo? _current = default!;

#if ANDROID
        protected override async Task OnInitializedAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/_content/Cyrena/mascots/mascots.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
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
#else
        protected override void OnInitialized()
        {
            var webHost = _services.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            if(webHost != null)
            InitializeFromWebHost(webHost);
            else
                InitializeFromMaui();
        }

        private void InitializeFromWebHost(Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env)
        {
            try
            {
                var inf = _env.WebRootFileProvider.GetFileInfo("_content/Cyrena/mascots/mascots.json");
                if (inf == null || string.IsNullOrEmpty(inf.PhysicalPath)) return;
                var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
                var json = File.ReadAllText(inf.PhysicalPath);
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

        private void InitializeFromMaui()
        {
            try
            {
                var inf = new FileInfo(Path.Combine(AppContext.BaseDirectory, "wwwroot/_content/Cyrena/mascots/mascots.json"));
                if (inf == null || string.IsNullOrEmpty(inf.FullName)) return;
                var customs = _settings.Read<Customization>(Customization.Key) ?? new Customization();
                var json = File.ReadAllText(inf.FullName);
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
#endif

        private string MascotPath()
        {
            if (_current == null)
                return "images/rene_f.png";
            var dir = Path.Combine("_content", "Cyrena", "mascots", _current.File!);
            return dir;
        }

        private void Previous()
        {
            var cidx = _current == null ? 0 : _mascots.FindIndex(x => x.File == _current.File);

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
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("file")]
        public string? File { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; } = 250;
    }
}
